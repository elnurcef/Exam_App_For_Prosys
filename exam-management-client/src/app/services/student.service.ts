import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Student, StudentRequest } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class StudentService {
  private readonly baseUrl = `${environment.apiBaseUrl}/students`;

  constructor(private readonly http: HttpClient) {}

  getAll(): Observable<Student[]> {
    return this.http.get<Student[]>(this.baseUrl);
  }

  getById(id: number): Observable<Student> {
    return this.http.get<Student>(`${this.baseUrl}/${id}`);
  }

  create(request: StudentRequest): Observable<Student> {
    return this.http.post<Student>(this.baseUrl, request);
  }

  update(id: number, request: StudentRequest): Observable<Student> {
    return this.http.put<Student>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
