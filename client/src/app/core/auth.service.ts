import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { AuthResponse, CurrentUser, JwtPayload } from './models';

const TOKEN_KEY = 'accessToken';
const REFRESH_TOKEN_KEY = 'refreshToken';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly currentUser = signal<CurrentUser | null>(this.readCurrentUser());

  readonly user = computed(() => this.currentUser());
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly isAdmin = computed(() => this.currentUser()?.roles.includes('Admin') ?? false);

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>('/api/Auth/login', { email, password }, { withCredentials: true })
      .pipe(tap((res) => this.storeTokens(res)));
  }

  register(userName: string, email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>('/api/Auth/register', { userName, email, password }, { withCredentials: true })
      .pipe(tap((res) => this.storeTokens(res)));
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      this.clearTokens();
      return;
    }

    this.http.post('/api/Auth/logout', { refreshToken }, { withCredentials: true }).subscribe({
      complete: () => this.clearTokens(),
      error: () => this.clearTokens(),
    });
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  private storeTokens(res: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, res.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, res.refreshToken);
    this.currentUser.set(this.decodeUser(res.accessToken));
  }

  private clearTokens(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    this.currentUser.set(null);
  }

  private decodeUser(token: string): CurrentUser {
    const payload = jwtDecode<JwtPayload>(token);

    return {
      id: payload.sub,
      email: payload.email,
      name: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
      roles: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
      expiresAt: new Date(payload.exp * 1000).toISOString(),
    };
  }

  private readCurrentUser(): CurrentUser | null {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) return null;

    try {
      const user = this.decodeUser(token);
      if (new Date(user.expiresAt) <= new Date()) {
        this.clearTokens();
        return null;
      }
      return user;
    } catch {
      this.clearTokens();
      return null;
    }
  }
}