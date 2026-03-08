import { Injectable } from '@angular/core';
import { Observable, of, tap, delay } from 'rxjs';

interface TokenResponse {
  access_token: string;
  refresh_token: string;
  expires_in: number;
  token_type: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {

  login(_username: string, _password: string): Observable<TokenResponse> {
    const fakeResponse: TokenResponse = {
      access_token: 'eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJfNDZJMW0wcnpMWEZSV2ZMbnBOUnpqSWJkZ0ctN05zZFZnR1BHX0l3azFNIn0.eyJleHAiOjE3NzMwMDQ1NzYsImlhdCI6MTc3Mjk2ODU3NiwianRpIjoib25sdHJvOmU2YTM4YjI3LWVkMTMtNjE0ZC05NWQ5LTZkNmE3NmMwYzRlNSIsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6ODA4MC9yZWFsbXMvbWFzdGVyIiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiYWRtaW4tY2xpIiwic2lkIjoiNDg0NTE4YjUtMzkzNy1hOWUzLWMyMGMtMmY4MmIyNzBjMTZkIiwic2NvcGUiOiJwcm9maWxlIGVtYWlsIn0.zU-dk_0dRnKr-L1X-8ZbS28N7Y6FC19cuipP1xFHWczbNs4CjWWoDLQnRjqHHBOCRl_3Tok5Yv_v62BfoEb-R0GyAZgyR-g5XXMCkMPc72I07yZD6sKLIamZ2oMhhXhECmid-kuh14iF0O-H47D3najnIovWogZDuiXQquf-PT6AcAW6XB_kr5zhM__3ZOvxoIN8gYb_zMUz8NE9RHcaK22igixPVRE_cM2vqQoxFwi0GN2i-KME57E6bVeu0tTzo7JOU828mvQudjEfpzqyFcQ2cd0GFQWsC_NByf4aWlQW6NutspoapCnp7-J47WPL6xO7nZprVgAa6G592Ldo5g',
      refresh_token: 'fake-refresh-token',
      expires_in: 36000,
      token_type: 'Bearer',
    };

    return of(fakeResponse).pipe(
      delay(500), // simulate network delay
      tap((res) => {
        localStorage.setItem('access_token', res.access_token);
        localStorage.setItem('refresh_token', res.refresh_token);
        console.log('Fake login successful, token stored.');
      })
    );
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  }

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}