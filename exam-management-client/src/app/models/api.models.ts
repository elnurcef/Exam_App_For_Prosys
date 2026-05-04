export type ExamStatus = 'Scheduled' | 'Graded' | 'Cancelled';

export interface Teacher {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  teacher: Teacher;
}

export interface Lesson {
  id: number;
  code: string;
  name: string;
  classLevel: number;
  teacherFirstName: string;
  teacherLastName: string;
  createdAt: string;
  updatedAt?: string | null;
}

export interface LessonRequest {
  code: string;
  name: string;
  classLevel: number;
}

export interface Student {
  id: number;
  number: number;
  firstName: string;
  lastName: string;
  classLevel: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface StudentRequest {
  number: number;
  firstName: string;
  lastName: string;
  classLevel: number;
}

export interface Exam {
  id: number;
  lessonId: number;
  lessonCode: string;
  lessonName: string;
  studentId: number;
  studentNumber: number;
  studentFullName: string;
  classLevel: number;
  examDate: string;
  grade?: number | null;
  status: ExamStatus;
  createdAt: string;
  updatedAt?: string | null;
}

export interface ExamScheduleRequest {
  lessonId: number;
  studentId: number;
  examDate: string;
}

export interface ExamGradeRequest {
  grade: number;
}

export interface ExamFilter {
  lessonId?: number | null;
  studentId?: number | null;
  classLevel?: number | null;
  examDate?: string | null;
  status?: ExamStatus | '' | null;
}

export interface ExamResult {
  examId: number;
  studentId: number;
  studentNumber: number;
  studentFullName: string;
  studentClass: number;
  lessonId: number;
  lessonCode: string;
  lessonName: string;
  examDate: string;
  grade?: number | null;
  status: ExamStatus;
}
