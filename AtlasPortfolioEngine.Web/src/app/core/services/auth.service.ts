import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `http://${window.location.hostname}:5146/api/v1/auth`;
  private readonly tokenKey = 'atlas_token';

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {}

  login(username: string, password: string) {
    return this.http
      .post<{ token: string }>(`${this.apiUrl}/token`, { username, password })
      .pipe(tap((response) => localStorage.setItem(this.tokenKey, response.token)));
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
