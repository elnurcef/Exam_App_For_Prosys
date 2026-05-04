import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Student } from '../../models/api.models';
import { StudentService } from '../../services/student.service';
import { getApiError } from '../../utils/api-error';

@Component({
  selector: 'app-student-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './student-form.component.html',
  styleUrl: './student-form.component.css'
})
export class StudentFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly studentService = inject(StudentService);

  @Input() student: Student | null = null;
  @Output() saved = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<void>();

  loading = false;
  error = '';

  readonly form = this.fb.nonNullable.group({
    number: [1, [Validators.required, Validators.min(1), Validators.max(99999)]],
    firstName: ['', [Validators.required, Validators.maxLength(30)]],
    lastName: ['', [Validators.required, Validators.maxLength(30)]],
    classLevel: [1, [Validators.required, Validators.min(1), Validators.max(12)]]
  });

  ngOnChanges(changes: SimpleChanges): void {
    if ('student' in changes) {
      if (this.student) {
        this.form.reset({
          number: this.student.number,
          firstName: this.student.firstName,
          lastName: this.student.lastName,
          classLevel: this.student.classLevel
        });
      } else {
        this.form.reset({ number: 1, firstName: '', lastName: '', classLevel: 1 });
      }
      this.error = '';
    }
  }

  submit(): void {
    this.error = '';
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.form.getRawValue();
    const action = this.student
      ? this.studentService.update(this.student.id, request)
      : this.studentService.create(request);

    this.loading = true;
    action.pipe(finalize(() => (this.loading = false))).subscribe({
      next: () => {
        const message = this.student ? 'Student updated successfully.' : 'Student created successfully.';
        this.form.reset({ number: 1, firstName: '', lastName: '', classLevel: 1 });
        this.saved.emit(message);
      },
      error: error => (this.error = getApiError(error))
    });
  }

  cancel(): void {
    this.form.reset({ number: 1, firstName: '', lastName: '', classLevel: 1 });
    this.error = '';
    this.cancelled.emit();
  }
}
