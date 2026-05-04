import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, Teacher } from '../models/api.models';

interface JwtPayload {
  exp?: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'exam_management_token';
  private readonly teacherKey = 'exam_management_teacher';
  private readonly teacherSubject = new BehaviorSubject<Teacher | null>(this.readStoredTeacher());
  readonly teacher$ = this.teacherSubject.asObservable();

  constructor(private readonly http: HttpClient, private readonly router: Router) {}

  signup(request: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    confirmPassword: string;
  }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/auth/signup`, request).pipe(
      tap(response => this.storeAuth(response))
    );
  }

  login(request: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiBaseUrl}/auth/login`, request).pipe(
      tap(response => this.storeAuth(response))
    );
  }

  me(): Observable<Teacher> {
    return this.http.get<Teacher>(`${environment.apiBaseUrl}/auth/me`).pipe(
      tap(teacher => {
        localStorage.setItem(this.teacherKey, JSON.stringify(teacher));
        this.teacherSubject.next(teacher);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.teacherKey);
    this.teacherSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token || this.isTokenExpired(token)) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.teacherKey);
      this.teacherSubject.next(null);
      return false;
    }

    return true;
  }

  private storeAuth(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.teacherKey, JSON.stringify(response.teacher));
    this.teacherSubject.next(response.teacher);
  }

  private readStoredTeacher(): Teacher | null {
    const raw = localStorage.getItem(this.teacherKey);
    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as Teacher;
    } catch {
      localStorage.removeItem(this.teacherKey);
      return null;
    }
  }

  private isTokenExpired(token: string): boolean {
    const payload = this.readPayload(token);
    if (!payload.exp) {
      return true;
    }

    return payload.exp * 1000 <= Date.now();
  }

  private readPayload(token: string): JwtPayload {
    try {
      const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(base64)
          .split('')
          .map(char => `%${(`00${char.charCodeAt(0).toString(16)}`).slice(-2)}`)
          .join('')
      );
      return JSON.parse(json) as JwtPayload;
    } catch {
      return {};
    }
  }
}
