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
  userId?: string | null;
}

const STORAGE_PREFERRED_NAME = 'auth.preferredName';
const STORAGE_USER_ID = 'auth.userId';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private backendBase = '';
  private frontendCallback = 'http://localhost:4200/auth-callback';

  // seed BehaviorSubjects from localStorage so values survive page refresh
  private _preferredName = new BehaviorSubject<string | null>(
    localStorage.getItem(STORAGE_PREFERRED_NAME)
  );
  public preferredName$ = this._preferredName.asObservable();

  private _userId = new BehaviorSubject<string | null>(
    localStorage.getItem(STORAGE_USER_ID)
  );
  public userId$ = this._userId.asObservable();

  private keepAliveTimer: number | null = null;

  constructor(private http: HttpClient) {}

  private setSession(preferredName: string | null, userId: string | null): void {
    this._preferredName.next(preferredName);
    this._userId.next(userId);

    if (preferredName) {
      localStorage.setItem(STORAGE_PREFERRED_NAME, preferredName);
    } else {
      localStorage.removeItem(STORAGE_PREFERRED_NAME);
    }

    if (userId) {
      localStorage.setItem(STORAGE_USER_ID, userId);
    } else {
      localStorage.removeItem(STORAGE_USER_ID);
    }
  }

  login(redirectUri?: string): Observable<void> {
    const body = { redirectUri: redirectUri ?? this.frontendCallback };
    return this.http
      .post<AuthorizationUrlResponse>(`${this.backendBase}/api/auth/authorize`, body, { withCredentials: true })
      .pipe(
        tap((res) => { window.location.assign(res.authorizationUrl); }),
        map(() => void 0)
      );
  }

  // Called after backend redirected to frontend callback
  handleFrontendCallback(): Observable<boolean> {
    return this.getSession().pipe(
      tap((session) => {
        this.setSession(session.preferredName ?? null, session.userId ?? null);
        this.scheduleSessionRefresh(session.expiresAt);
      }),
      map(() => true),
      catchError(() => {
        this.setSession(null, null);
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
        this.setSession(null, null);
        this.clearScheduledRefresh();
        if (res?.logoutUrl) window.location.href = res.logoutUrl;
      }),
      map(() => void 0),
      catchError(() => {
        this.setSession(null, null);
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

  getUserIdSync(): string | null {
    return this._userId.getValue();
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
        const session = await firstValueFrom(this.getSession());
        this.setSession(session.preferredName ?? null, session.userId ?? null);
        this.scheduleSessionRefresh(session.expiresAt);
      } catch {
        this.setSession(null, null);
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