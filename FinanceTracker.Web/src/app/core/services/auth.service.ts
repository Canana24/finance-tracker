import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, TokenResponse } from '../models/auth.model';

const TOKEN_KEY = 'ft_token';

@Injectable({
  providedIn: 'root'
})

export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);

    private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));

    readonly isLoggedIn = computed(() => this._token() !== null);

    login(credentials: LoginRequest) {

        return this.http
            .post<TokenResponse>(`${environment.apiUrl}/auth/login`, credentials)
            .pipe(tap((response) => this.storeToken(response.token)));
    }

    register(data: RegisterRequest) {
        return this.http
            .post<TokenResponse>(`${environment.apiUrl}/auth/register`, data)
            .pipe(tap((response) => this.storeToken(response.token)));
    }

    logout() {
        localStorage.removeItem(TOKEN_KEY);
        this._token.set(null);
        this.router.navigate(['/login']);
    }

    getToken(): string | null
    {
        return this._token();
    }

    private storeToken(token: string) {
        localStorage.setItem(TOKEN_KEY, token);
        this._token.set(token);
    }
}