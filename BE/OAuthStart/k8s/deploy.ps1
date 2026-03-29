<#
  Deploy the Auction app to Minikube.
  Prerequisites:
    - minikube start --driver=docker --memory=4096 --cpus=4
    - minikube addons enable ingress
#>
param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

# ─── Resolve paths from k8s/ folder location ───
# k8s dir:       repo/BE/OAuthStart/k8s
# OAuthStart:    repo/BE/OAuthStart          (build context for .NET Dockerfiles)
# BE dir:        repo/BE                      (docker-compose.yml, certs, themes, realm-export, init-db.sql)
# repo root:     repo                         (contains BE/ and FE/)
$k8sDir       = $PSScriptRoot
$oauthStartDir = Split-Path -Parent $k8sDir                    # repo/BE/OAuthStart
$beDir         = Split-Path -Parent $oauthStartDir              # repo/BE
$repoDir       = Split-Path -Parent $beDir                      # repo

Write-Host "Paths:" -ForegroundColor DarkGray
Write-Host "  k8s:        $k8sDir" -ForegroundColor DarkGray
Write-Host "  OAuthStart: $oauthStartDir" -ForegroundColor DarkGray
Write-Host "  BE:         $beDir" -ForegroundColor DarkGray
Write-Host "  Repo:       $repoDir" -ForegroundColor DarkGray

# ─────────────────────────────────────────────
# 0. Ensure Minikube is running & kubectl context is set
# ─────────────────────────────────────────────
Write-Host "`n=== Checking Minikube status ===" -ForegroundColor Cyan
$mkStatus = & minikube status --format='{{.Host}}' 2>&1
if ($mkStatus -notmatch "Running") {
    Write-Host "Minikube is not running. Starting..." -ForegroundColor Yellow
    & minikube start --driver=docker --memory=4096 --cpus=4
}
& kubectl config use-context minikube

# ─────────────────────────────────────────────
# 1. Point Docker CLI at Minikube's daemon
# ─────────────────────────────────────────────
Write-Host "`n=== Switching to Minikube Docker daemon ===" -ForegroundColor Cyan
& minikube -p minikube docker-env --shell powershell | Where-Object { $_ -match '^\$Env:' } | ForEach-Object { Invoke-Expression $_ }

# ─────────────────────────────────────────────
# 2. Build images
# ─────────────────────────────────────────────
if (-not $SkipBuild) {
    Write-Host "`n=== Building .NET images (inside Minikube daemon) ===" -ForegroundColor Cyan

    Write-Host "  Building services-hoster..." -ForegroundColor DarkGray
    docker build -t services-hoster:local `
        -f "$oauthStartDir/ServicesHoster/Dockerfile" $oauthStartDir

    Write-Host "  Building oauth-service..." -ForegroundColor DarkGray
    docker build -t oauth-service:local `
        -f "$oauthStartDir/OAuthCodeFlowService/Dockerfile" $oauthStartDir

    Write-Host "  Building payments..." -ForegroundColor DarkGray
    docker build -t payments:local `
        -f "$oauthStartDir/Payments/Dockerfile" $oauthStartDir

    # ── Frontend: build with LOCAL Docker daemon, then load into Minikube ──
    # Minikube's daemon can't pull from Docker Hub due to corporate TLS interception.
    Write-Host "`n=== Building frontend (local Docker daemon) ===" -ForegroundColor Cyan

    # Temporarily restore local Docker env
    $savedDockerHost     = $Env:DOCKER_HOST
    $savedDockerCert     = $Env:DOCKER_CERT_PATH
    $savedDockerTls      = $Env:DOCKER_TLS_VERIFY
    $savedMinikubeActive = $Env:MINIKUBE_ACTIVE_DOCKERD

    Remove-Item Env:DOCKER_HOST -ErrorAction SilentlyContinue
    Remove-Item Env:DOCKER_CERT_PATH -ErrorAction SilentlyContinue
    Remove-Item Env:DOCKER_TLS_VERIFY -ErrorAction SilentlyContinue
    Remove-Item Env:MINIKUBE_ACTIVE_DOCKERD -ErrorAction SilentlyContinue

    $feCtx = Join-Path $repoDir "FE/auction"
    Write-Host "  Building frontend from $feCtx ..." -ForegroundColor DarkGray
    docker build -t frontend:local -f "$feCtx/Dockerfile" $feCtx

    Write-Host "  Loading frontend image into Minikube..." -ForegroundColor DarkGray
    minikube image load frontend:local

    # Restore Minikube Docker env
    if ($savedDockerHost)     { $Env:DOCKER_HOST = $savedDockerHost }
    if ($savedDockerCert)     { $Env:DOCKER_CERT_PATH = $savedDockerCert }
    if ($savedDockerTls)      { $Env:DOCKER_TLS_VERIFY = $savedDockerTls }
    if ($savedMinikubeActive) { $Env:MINIKUBE_ACTIVE_DOCKERD = $savedMinikubeActive }
}

# ─────────────────────────────────────────────
# 3. Apply namespace FIRST and wait for it
# ─────────────────────────────────────────────
Write-Host "`n=== Creating namespace ===" -ForegroundColor Cyan
kubectl apply -f "$k8sDir/00-namespace.yaml"
# Ensure the namespace is fully created before proceeding
kubectl wait --for=jsonpath='{.status.phase}'=Active namespace/auction --timeout=30s

# ─────────────────────────────────────────────
# 4. Create realm-export ConfigMap from file
# ─────────────────────────────────────────────
$realmFile = Join-Path $beDir "realm-export/realm-export.json"
if (Test-Path $realmFile) {
    Write-Host "=== Creating keycloak-realm-export ConfigMap ===" -ForegroundColor Cyan
    kubectl create configmap keycloak-realm-export `
        --from-file=realm-export.json=$realmFile `
        -n auction --dry-run=client -o yaml | kubectl apply -f -
} else {
    Write-Warning "realm-export.json not found at $realmFile — Keycloak won't import a realm."
}

# ─────────────────────────────────────────────
# 5. Copy themes into Minikube VM
# ─────────────────────────────────────────────
$themesDir = Join-Path $beDir "themes"
if (Test-Path $themesDir) {
    Write-Host "=== Copying themes into Minikube ===" -ForegroundColor Cyan
    minikube cp "$themesDir" /mnt/keycloak-themes
}

# ─────────────────────────────────────────────
# 6. Create TLS secret for Ingress
# ─────────────────────────────────────────────
$certDir = Join-Path $beDir "certs"
if (Test-Path "$certDir/localhost.crt") {
    Write-Host "=== Creating TLS secret ===" -ForegroundColor Cyan
    kubectl create secret tls auction-tls `
        --cert="$certDir/localhost.crt" `
        --key="$certDir/localhost.key" `
        -n auction --dry-run=client -o yaml | kubectl apply -f -
} else {
    Write-Warning "No certs found at $certDir — Ingress TLS won't work."
}

# ─────────────────────────────────────────────
# 7. Apply all manifests in order (only numbered files)
# ─────────────────────────────────────────────
Write-Host "`n=== Applying K8s manifests ===" -ForegroundColor Cyan
Get-ChildItem "$k8sDir/[0-9]*.yaml" | Sort-Object Name | ForEach-Object {
    Write-Host "  Applying $($_.Name)" -ForegroundColor DarkGray
    kubectl apply -f $_.FullName
}

# ─────────────────────────────────────────────
# 8. Wait & show status
# ─────────────────────────────────────────────
Write-Host "`n=== Waiting for pods ===" -ForegroundColor Cyan
kubectl rollout status deployment/postgres        -n auction --timeout=120s
kubectl rollout status deployment/keycloak        -n auction --timeout=300s
kubectl rollout status deployment/services-hoster -n auction --timeout=120s
kubectl rollout status deployment/oauth-service   -n auction --timeout=120s
kubectl rollout status deployment/payments        -n auction --timeout=60s
kubectl rollout status deployment/frontend        -n auction --timeout=60s

$minikubeIp = & minikube ip 2>&1

Write-Host "`n=== DONE ===" -ForegroundColor Green
Write-Host ""
Write-Host "Add this line to C:\Windows\System32\drivers\etc\hosts (run as Admin):" -ForegroundColor Yellow
Write-Host "  $minikubeIp auction.local keycloak.local" -ForegroundColor White
Write-Host ""
Write-Host "Then open:" -ForegroundColor Yellow
Write-Host "  App:      https://auction.local" -ForegroundColor White
Write-Host "  Keycloak: http://keycloak.local  (admin/admin)" -ForegroundColor White
Write-Host ""