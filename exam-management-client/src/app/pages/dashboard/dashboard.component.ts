import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { Exam, Lesson, Student } from '../../models/api.models';
import { AuthService } from '../../services/auth.service';
import { ExamService } from '../../services/exam.service';
import { LessonService } from '../../services/lesson.service';
import { StudentService } from '../../services/student.service';
import { getApiError } from '../../utils/api-error';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly lessonService = inject(LessonService);
  private readonly studentService = inject(StudentService);
  private readonly examService = inject(ExamService);

  lessons: Lesson[] = [];
  students: Student[] = [];
  exams: Exam[] = [];
  loading = false;
  error = '';
  readonly teacher$ = this.authService.teacher$;

  ngOnInit(): void {
    this.load();
    this.authService.me().subscribe({ error: () => undefined });
  }

  get scheduledCount(): number {
    return this.exams.filter(exam => exam.status === 'Scheduled').length;
  }

  get gradedCount(): number {
    return this.exams.filter(exam => exam.status === 'Graded').length;
  }

  get recentExams(): Exam[] {
    return this.exams.slice(0, 5);
  }

  load(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      lessons: this.lessonService.getAll(),
      students: this.studentService.getAll(),
      exams: this.examService.getAll()
    })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: result => {
          this.lessons = result.lessons;
          this.students = result.students;
          this.exams = result.exams;
        },
        error: error => (this.error = getApiError(error))
      });
  }
}
