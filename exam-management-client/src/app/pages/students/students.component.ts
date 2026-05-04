import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs';
import { ConfirmDialogComponent } from '../../components/confirm-dialog/confirm-dialog.component';
import { Student } from '../../models/api.models';
import { StudentService } from '../../services/student.service';
import { getApiError } from '../../utils/api-error';
import { StudentFormComponent } from '../student-form/student-form.component';

@Component({
  selector: 'app-students',
  standalone: true,
  imports: [CommonModule, StudentFormComponent, ConfirmDialogComponent],
  templateUrl: './students.component.html',
  styleUrl: './students.component.css'
})
export class StudentsComponent implements OnInit {
  students: Student[] = [];
  selectedStudent: Student | null = null;
  viewedStudent: Student | null = null;
  pendingDelete: Student | null = null;
  loading = false;
  deleting = false;
  error = '';
  success = '';

  constructor(private readonly studentService: StudentService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    this.studentService.getAll()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: students => (this.students = students),
        error: error => (this.error = getApiError(error))
      });
  }

  edit(student: Student): void {
    this.selectedStudent = student;
    this.viewedStudent = null;
    this.success = '';
  }

  view(student: Student): void {
    this.viewedStudent = student;
    this.selectedStudent = null;
  }

  onSaved(message: string): void {
    this.selectedStudent = null;
    this.success = message;
    this.load();
  }

  confirmDelete(student: Student): void {
    this.pendingDelete = student;
    this.success = '';
    this.error = '';
  }

  deleteConfirmed(): void {
    if (!this.pendingDelete) {
      return;
    }

    this.deleting = true;
    this.studentService.delete(this.pendingDelete.id)
      .pipe(finalize(() => (this.deleting = false)))
      .subscribe({
        next: () => {
          this.success = 'Student deleted successfully.';
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
