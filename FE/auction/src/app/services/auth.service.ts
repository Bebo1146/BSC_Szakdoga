import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, map, catchError, tap } from 'rxjs';
import { firstValueFrom } from 'rxjs';

interface AuthorizationUrlResponse {
  authorizationUrl: string;
  state: string;
}

export interface MeResponse {
  expiresAt: string;
  hasRefreshToken: boolean;
  preferredName?: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private backendBase = 'http://localhost:5215';
  private frontendCallback = 'http://localhost:4200/auth-callback';

  private static readonly PreferredNameKey = 'app.preferredName';

  private _preferredName = new BehaviorSubject<string | null>(AuthService.readPreferredNameFromStorage());
  public preferredName$ = this._preferredName.asObservable();

  // timer id for scheduled refresh
  private keepAliveTimer: number | null = null;

  constructor(private http: HttpClient) {}

  private static readPreferredNameFromStorage(): string | null {
    try {
      return localStorage.getItem(AuthService.PreferredNameKey) ?? null;
    } catch {
      return null;
    }
  }

  private static writePreferredNameToStorage(name: string | null): void {
    try {
      if (name === null) localStorage.removeItem(AuthService.PreferredNameKey);
      else localStorage.setItem(AuthService.PreferredNameKey, name);
    } catch {
      // ignore storage errors
    }
  }

  login(redirectUri?: string): Observable<void> {
    const body = { redirectUri: redirectUri ?? this.frontendCallback };
    return this.http
      .post<AuthorizationUrlResponse>(`${this.backendBase}/api/auth/authorize`, body, { withCredentials: true })
      .pipe(
        tap((res) => {
          window.location.assign(res.authorizationUrl);
        }),
        map(() => void 0)
      );
  }

  // Called after backend redirected to frontend callback
  handleFrontendCallback(): Observable<boolean> {
    return this.getSession().pipe(
      tap((session) => {
        const name = session.preferredName ?? null;
        this._preferredName.next(name);
        AuthService.writePreferredNameToStorage(name);

        // schedule keep-alive based on expiresAt returned by server
        this.scheduleSessionRefresh(session.expiresAt);
      }),
      map(() => true),
      catchError(() => {
        this._preferredName.next(null);
        AuthService.writePreferredNameToStorage(null);
        this.clearScheduledRefresh();
        return of(false);
      })
    );
  }

  getSession(): Observable<MeResponse> {
    return this.http.get<MeResponse>(`${this.backendBase}/api/auth/me`, { withCredentials: true });
  }

  logout(): Observable<void> {
    return this.http.get<{ logoutUrl: string }>(`${this.backendBase}/api/auth/logout-url`, { withCredentials: true }).pipe(
      tap((res) => {
        this._preferredName.next(null);
        AuthService.writePreferredNameToStorage(null);
        this.clearScheduledRefresh();
        if (res?.logoutUrl) window.location.href = res.logoutUrl;
      }),
      map(() => void 0),
      catchError(() => {
        this._preferredName.next(null);
        AuthService.writePreferredNameToStorage(null);
        this.clearScheduledRefresh();
        return of(void 0);
      })
    );
  }

  isLoggedIn(): Observable<boolean> {
    return this.getSession().pipe(map(() => true), catchError(() => of(false)));
  }

  getPreferredNameSync(): string | null {
    return this._preferredName.getValue();
  }

  // --- keep-alive scheduling ------------------------------------------------
  private scheduleSessionRefresh(expiresAtIso: string | undefined | null): void {
    this.clearScheduledRefresh();
    if (!expiresAtIso) return;

    const expiresAt = Date.parse(expiresAtIso);
    if (isNaN(expiresAt)) return;

    const now = Date.now();
    // target refresh time: 60s before expiry, but at least 10s in the future
    const target = Math.max(expiresAt - 60_000, now + 10_000);
    const delay = Math.max(0, target - now);

    this.keepAliveTimer = window.setTimeout(async () => {
      try {
        // call backend to refresh session info (server refreshes tokens on demand)
        const session = await firstValueFrom(this.getSession());
        const name = session.preferredName ?? null;
        this._preferredName.next(name);
        AuthService.writePreferredNameToStorage(name);
        // reschedule based on new expiry
        this.scheduleSessionRefresh(session.expiresAt);
      } catch {
        // on failure clear local state — user will be redirected by interceptor or UI
        this._preferredName.next(null);
        AuthService.writePreferredNameToStorage(null);
        this.clearScheduledRefresh();
      }
    }, delay);
  }

  private clearScheduledRefresh(): void {
    if (this.keepAliveTimer !== null) {
      clearTimeout(this.keepAliveTimer);
      this.keepAliveTimer = null;
    }
  }
}