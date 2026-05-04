import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ExamFilter, ExamResult } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly baseUrl = `${environment.apiBaseUrl}/reports`;

  constructor(private readonly http: HttpClient) {}

  getExamResults(filter: ExamFilter = {}): Observable<ExamResult[]> {
    return this.http.get<ExamResult[]>(`${this.baseUrl}/exam-results`, { params: this.toParams(filter) });
  }

  getStudentReport(studentId: number, filter: ExamFilter = {}): Observable<ExamResult[]> {
    return this.http.get<ExamResult[]>(`${this.baseUrl}/student/${studentId}`, { params: this.toParams(filter) });
  }

  getLessonReport(lessonId: number, filter: ExamFilter = {}): Observable<ExamResult[]> {
    return this.http.get<ExamResult[]>(`${this.baseUrl}/lesson/${lessonId}`, { params: this.toParams(filter) });
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
