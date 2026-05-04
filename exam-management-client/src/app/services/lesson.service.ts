import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Lesson, LessonRequest } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class LessonService {
  private readonly baseUrl = `${environment.apiBaseUrl}/lessons`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Lesson[]> {
    return this.http.get<Lesson[]>(this.baseUrl);
  }

  getById(id: number): Observable<Lesson> {
    return this.http.get<Lesson>(`${this.baseUrl}/${id}`);
  }

  create(request: LessonRequest): Observable<Lesson> {
    return this.http.post<Lesson>(this.baseUrl, request);
  }

  update(id: number, request: LessonRequest): Observable<Lesson> {
    return this.http.put<Lesson>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
