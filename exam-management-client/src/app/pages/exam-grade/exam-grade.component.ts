import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Exam } from '../../models/api.models';
import { ExamService } from '../../services/exam.service';
import { getApiError } from '../../utils/api-error';

@Component({
  selector: 'app-exam-grade',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './exam-grade.component.html',
  styleUrl: './exam-grade.component.css'
})
export class ExamGradeComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly examService = inject(ExamService);

  @Input() exam: Exam | null = null;
  @Output() saved = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<void>();

  loading = false;
  error = '';

  readonly form = this.fb.group({
    grade: [null as number | null, [Validators.required, Validators.min(0), Validators.max(5), ExamGradeComponent.integerValidator]]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if ('exam' in changes && this.exam) {
      this.form.reset({ grade: this.exam.grade ?? null });
      this.error = this.exam.status === 'Cancelled' ? 'Cancelled exam cannot be graded.' : '';
    }
  }

  submit(): void {
    this.error = '';
    if (!this.exam) {
      return;
    }

    if (this.exam.status === 'Cancelled') {
      this.error = 'Cancelled exam cannot be graded.';
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const grade = Number(this.form.value.grade);
    this.loading = true;
    this.examService.grade(this.exam.id, { grade })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.saved.emit('Exam grade saved successfully.'),
        error: error => (this.error = getApiError(error))
      });
  }

  cancel(): void {
    this.error = '';
    this.cancelled.emit();
  }

  private static integerValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (value === null || value === undefined || value === '') {
      return null;
    }

    return Number.isInteger(Number(value)) ? null : { integer: true };
  }
}
