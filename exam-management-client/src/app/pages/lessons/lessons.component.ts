import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { ConfirmDialogComponent } from '../../components/confirm-dialog/confirm-dialog.component';
import { Lesson } from '../../models/api.models';
import { LessonService } from '../../services/lesson.service';
import { getApiError } from '../../utils/api-error';
import { LessonFormComponent } from '../lesson-form/lesson-form.component';

@Component({
  selector: 'app-lessons',
  standalone: true,
  imports: [CommonModule, LessonFormComponent, ConfirmDialogComponent],
  templateUrl: './lessons.component.html',
  styleUrl: './lessons.component.css'
})
export class LessonsComponent implements OnInit {
  lessons: Lesson[] = [];
  selectedLesson: Lesson | null = null;
  viewedLesson: Lesson | null = null;
  pendingDelete: Lesson | null = null;
  loading = false;
  deleting = false;
  error = '';
  success = '';

  constructor(private readonly lessonService: LessonService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.lessonService.getAll()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: lessons => (this.lessons = lessons),
        error: error => (this.error = getApiError(error))
      });
  }

  edit(lesson: Lesson): void {
    this.selectedLesson = lesson;
    this.viewedLesson = null;
    this.success = '';
  }

  view(lesson: Lesson): void {
    this.viewedLesson = lesson;
    this.selectedLesson = null;
  }

  onSaved(message: string): void {
    this.selectedLesson = null;
    this.success = message;
    this.load();
  }

  confirmDelete(lesson: Lesson): void {
    this.pendingDelete = lesson;
    this.success = '';
    this.error = '';
  }

  deleteConfirmed(): void {
    if (!this.pendingDelete) {
      return;
    }

    this.deleting = true;
    this.lessonService.delete(this.pendingDelete.id)
      .pipe(finalize(() => (this.deleting = false)))
      .subscribe({
        next: () => {
          this.success = 'Lesson deleted successfully.';
          this.pendingDelete = null;
          this.load();
        },
        error: error => {
          this.error = getApiError(error);
          this.pendingDelete = null;
        }
      });
  }
}
