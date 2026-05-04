import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { ConfirmDialogComponent } from '../../components/confirm-dialog/confirm-dialog.component';
import { Exam, ExamFilter, Lesson, Student } from '../../models/api.models';
import { ExamService } from '../../services/exam.service';
import { LessonService } from '../../services/lesson.service';
import { StudentService } from '../../services/student.service';
import { getApiError } from '../../utils/api-error';
import { ExamGradeComponent } from '../exam-grade/exam-grade.component';

@Component({
  selector: 'app-exams',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ConfirmDialogComponent, ExamGradeComponent],
  templateUrl: './exams.component.html',
  styleUrl: './exams.component.css'
})
export class ExamsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly lessonService = inject(LessonService);
  private readonly studentService = inject(StudentService);
  private readonly examService = inject(ExamService);

  exams: Exam[] = [];
  lessons: Lesson[] = [];
  students: Student[] = [];
  selectedExam: Exam | null = null;
  viewedExam: Exam | null = null;
  gradingExam: Exam | null = null;
  pendingCancel: Exam | null = null;
  pendingDelete: Exam | null = null;
  loading = false;
  saving = false;
  error = '';
  success = '';

  readonly filters = this.fb.group({
    lessonId: [null as number | null],
    studentId: [null as number | null],
    classLevel: [null as number | null],
    examDate: [''],
    status: ['']
  });

  readonly editForm = this.fb.group({
    lessonId: [null as number | null, [Validators.required]],
    studentId: [null as number | null, [Validators.required]],
    examDate: ['', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadAll();
  }

  get selectedLessonForEdit(): Lesson | undefined {
    return this.lessons.find(lesson => lesson.id === Number(this.editForm.value.lessonId));
  }

  get selectedStudentForEdit(): Student | undefined {
    return this.students.find(student => student.id === Number(this.editForm.value.studentId));
  }

  get editClassMismatch(): boolean {
    return !!this.selectedLessonForEdit
      && !!this.selectedStudentForEdit
      && this.selectedLessonForEdit.classLevel !== this.selectedStudentForEdit.classLevel;
  }

  loadAll(): void {
    this.loading = true;
    this.error = '';
    forkJoin({
      lessons: this.lessonService.getAll(),
      students: this.studentService.getAll()
    }).subscribe({
      next: result => {
        this.lessons = result.lessons;
        this.students = result.students;
        this.loadExams();
      },
      error: error => {
        this.loading = false;
        this.error = getApiError(error);
      }
    });
  }

  loadExams(): void {
    this.loading = true;
    this.examService.getAll(this.currentFilter())
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: exams => (this.exams = exams),
        error: error => (this.error = getApiError(error))
      });
  }

  resetFilters(): void {
    this.filters.reset({ lessonId: null, studentId: null, classLevel: null, examDate: '', status: '' });
    this.loadExams();
  }

  view(exam: Exam): void {
    this.viewedExam = exam;
    this.selectedExam = null;
    this.gradingExam = null;
  }

  edit(exam: Exam): void {
    this.selectedExam = exam;
    this.viewedExam = null;
    this.gradingExam = null;
    this.editForm.reset({
      lessonId: exam.lessonId,
      studentId: exam.studentId,
      examDate: exam.examDate
    });
  }

  saveEdit(): void {
    this.error = '';
    this.success = '';
    if (!this.selectedExam) {
      return;
    }

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    if (this.editClassMismatch) {
      this.error = 'Student class must match lesson class.';
      return;
    }

    this.saving = true;
    this.examService.update(this.selectedExam.id, {
      lessonId: Number(this.editForm.value.lessonId),
      studentId: Number(this.editForm.value.studentId),
      examDate: this.editForm.value.examDate ?? ''
    })
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: () => {
          this.success = 'Exam updated successfully.';
          this.selectedExam = null;
          this.loadExams();
        },
        error: error => (this.error = getApiError(error))
      });
  }

  grade(exam: Exam): void {
    this.gradingExam = exam;
    this.selectedExam = null;
    this.viewedExam = null;
  }

  onGraded(message: string): void {
    this.success = message;
    this.gradingExam = null;
    this.loadExams();
  }

  cancelExamConfirmed(): void {
    if (!this.pendingCancel) {
      return;
    }

    this.examService.cancel(this.pendingCancel.id).subscribe({
      next: () => {
        this.success = 'Exam cancelled successfully.';
        this.pendingCancel = null;
        this.loadExams();
      },
      error: error => {
        this.error = getApiError(error);
        this.pendingCancel = null;
      }
    });
  }

  deleteExamConfirmed(): void {
    if (!this.pendingDelete) {
      return;
    }

    this.examService.delete(this.pendingDelete.id).subscribe({
      next: () => {
        this.success = 'Cancelled exam deleted successfully.';
        this.pendingDelete = null;
        this.loadExams();
      },
      error: error => {
        this.error = getApiError(error);
        this.pendingDelete = null;
      }
    });
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
