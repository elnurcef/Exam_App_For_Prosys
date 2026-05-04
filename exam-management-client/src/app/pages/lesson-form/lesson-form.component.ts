import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { Lesson } from '../../models/api.models';
import { LessonService } from '../../services/lesson.service';
import { getApiError } from '../../utils/api-error';

@Component({
  selector: 'app-lesson-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './lesson-form.component.html',
  styleUrl: './lesson-form.component.css'
})
export class LessonFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly lessonService = inject(LessonService);

  @Input() lesson: Lesson | null = null;
  @Output() saved = new EventEmitter<string>();
  @Output() cancelled = new EventEmitter<void>();

  loading = false;
  error = '';

  readonly form = this.fb.nonNullable.group({
    code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(3), Validators.pattern(/^[A-Z0-9]{3}$/)]],
    name: ['', [Validators.required, Validators.maxLength(30)]],
    classLevel: [1, [Validators.required, Validators.min(1), Validators.max(12)]]
  });

  constructor() {
    this.form.controls.code.valueChanges.subscribe(value => {
      const uppercase = value.toUpperCase();
      if (value !== uppercase) {
        this.form.controls.code.setValue(uppercase, { emitEvent: false });
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('lesson' in changes) {
      if (this.lesson) {
        this.form.reset({
          code: this.lesson.code,
          name: this.lesson.name,
          classLevel: this.lesson.classLevel
        });
      } else {
        this.form.reset({ code: '', name: '', classLevel: 1 });
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
    const action = this.lesson
      ? this.lessonService.update(this.lesson.id, request)
      : this.lessonService.create(request);

    this.loading = true;
    action.pipe(finalize(() => (this.loading = false))).subscribe({
      next: () => {
        const message = this.lesson ? 'Lesson updated successfully.' : 'Lesson created successfully.';
        this.form.reset({ code: '', name: '', classLevel: 1 });
        this.saved.emit(message);
      },
      error: error => (this.error = getApiError(error))
    });
  }

  cancel(): void {
    this.form.reset({ code: '', name: '', classLevel: 1 });
    this.error = '';
    this.cancelled.emit();
  }
}
