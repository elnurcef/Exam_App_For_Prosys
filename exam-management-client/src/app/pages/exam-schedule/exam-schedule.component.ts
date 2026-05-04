import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { Lesson, Student } from '../../models/api.models';
import { ExamService } from '../../services/exam.service';
import { LessonService } from '../../services/lesson.service';
import { StudentService } from '../../services/student.service';
import { getApiError } from '../../utils/api-error';

@Component({
  selector: 'app-exam-schedule',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './exam-schedule.component.html',
  styleUrl: './exam-schedule.component.css'
})
export class ExamScheduleComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly lessonService = inject(LessonService);
  private readonly studentService = inject(StudentService);
  private readonly examService = inject(ExamService);
  private readonly router = inject(Router);

  lessons: Lesson[] = [];
  students: Student[] = [];
  loading = false;
  saving = false;
  error = '';
  success = '';

  readonly form = this.fb.group({
    lessonId: [null as number | null, [Validators.required]],
    studentId: [null as number | null, [Validators.required]],
    examDate: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadReferences();
  }

  get selectedLesson(): Lesson | undefined {
    return this.lessons.find(lesson => lesson.id === Number(this.form.value.lessonId));
  }

  get selectedStudent(): Student | undefined {
    return this.students.find(student => student.id === Number(this.form.value.studentId));
  }

  get classMismatch(): boolean {
    return !!this.selectedLesson && !!this.selectedStudent && this.selectedLesson.classLevel !== this.selectedStudent.classLevel;
  }

  loadReferences(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      lessons: this.lessonService.getAll(),
      students: this.studentService.getAll()
    })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: result => {
          this.lessons = result.lessons;
          this.students = result.students;
        },
        error: error => (this.error = getApiError(error))
      });
  }

  submit(): void {
    this.error = '';
    this.success = '';
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.classMismatch) {
      this.error = 'Student class must match lesson class.';
      return;
    }

    const lessonId = Number(this.form.value.lessonId);
    const studentId = Number(this.form.value.studentId);
    const examDate = this.form.value.examDate ?? '';

    this.saving = true;
    this.examService.schedule({ lessonId, studentId, examDate })
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: () => {
          this.success = 'Exam scheduled successfully.';
          this.form.reset({ lessonId: null, studentId: null, examDate: '' });
        },
        error: error => (this.error = getApiError(error))
      });
  }

  goToExams(): void {
    this.router.navigate(['/exams']);
  }
}
