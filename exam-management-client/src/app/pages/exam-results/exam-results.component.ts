import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { forkJoin, finalize } from 'rxjs';
import { ExamFilter, ExamResult, Lesson, Student } from '../../models/api.models';
import { LessonService } from '../../services/lesson.service';
import { ReportService } from '../../services/report.service';
import { StudentService } from '../../services/student.service';
import { getApiError } from '../../utils/api-error';

@Component({
  selector: 'app-exam-results',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './exam-results.component.html',
  styleUrl: './exam-results.component.css'
})
export class ExamResultsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly lessonService = inject(LessonService);
  private readonly studentService = inject(StudentService);
  private readonly reportService = inject(ReportService);

  results: ExamResult[] = [];
  lessons: Lesson[] = [];
  students: Student[] = [];
  loading = false;
  error = '';

  readonly filters = this.fb.group({
    lessonId: [null as number | null],
    studentId: [null as number | null],
    classLevel: [null as number | null],
    examDate: [''],
    status: ['']
  });

  ngOnInit(): void {
    this.loading = true;
    forkJoin({
      lessons: this.lessonService.getAll(),
      students: this.studentService.getAll()
    }).subscribe({
      next: result => {
        this.lessons = result.lessons;
        this.students = result.students;
        this.loadResults();
      },
      error: error => {
        this.loading = false;
        this.error = getApiError(error);
      }
    });
  }

  loadResults(): void {
    this.loading = true;
    this.error = '';
    this.reportService.getExamResults(this.currentFilter())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: results => (this.results = results),
        error: error => (this.error = getApiError(error))
      });
  }

  resetFilters(): void {
    this.filters.reset({ lessonId: null, studentId: null, classLevel: null, examDate: '', status: '' });
    this.loadResults();
  }

  private currentFilter(): ExamFilter {
    const raw = this.filters.value;
    return {
      lessonId: raw.lessonId ? Number(raw.lessonId) : null,
      studentId: raw.studentId ? Number(raw.studentId) : null,
      classLevel: raw.classLevel ? Number(raw.classLevel) : null,
      examDate: raw.examDate || null,
      status: raw.status as ExamFilter['status']
    };
  }
}
