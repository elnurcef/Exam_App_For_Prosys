import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Exam, ExamFilter, ExamGradeRequest, ExamScheduleRequest } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ExamService {
  private readonly baseUrl = `${environment.apiBaseUrl}/exams`;

  constructor(private readonly http: HttpClient) {}

  getAll(filter: ExamFilter = {}): Observable<Exam[]> {
    return this.http.get<Exam[]>(this.baseUrl, { params: this.toParams(filter) });
  }

  getById(id: number): Observable<Exam> {
    return this.http.get<Exam>(`${this.baseUrl}/${id}`);
  }

  schedule(request: ExamScheduleRequest): Observable<Exam> {
    return this.http.post<Exam>(`${this.baseUrl}/schedule`, request);
  }

  update(id: number, request: ExamScheduleRequest): Observable<Exam> {
    return this.http.put<Exam>(`${this.baseUrl}/${id}`, request);
  }

  grade(id: number, request: ExamGradeRequest): Observable<Exam> {
    return this.http.put<Exam>(`${this.baseUrl}/${id}/grade`, request);
  }

  cancel(id: number): Observable<Exam> {
    return this.http.put<Exam>(`${this.baseUrl}/${id}/cancel`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  private toParams(filter: ExamFilter): HttpParams {
    let params = new HttpParams();
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    });

    return params;
  }
}
