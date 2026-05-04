import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './guards/auth.guard';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ExamResultsComponent } from './pages/exam-results/exam-results.component';
import { ExamScheduleComponent } from './pages/exam-schedule/exam-schedule.component';
import { ExamsComponent } from './pages/exams/exams.component';
import { LessonsComponent } from './pages/lessons/lessons.component';
import { LoginComponent } from './pages/login/login.component';
import { SignupComponent } from './pages/signup/signup.component';
import { StudentsComponent } from './pages/students/students.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'signup', component: SignupComponent, canActivate: [guestGuard] },
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { path: 'lessons', component: LessonsComponent, canActivate: [authGuard] },
  { path: 'students', component: StudentsComponent, canActivate: [authGuard] },
  { path: 'exams', component: ExamsComponent, canActivate: [authGuard] },
  { path: 'exams/schedule', component: ExamScheduleComponent, canActivate: [authGuard] },
  { path: 'exams/results', component: ExamResultsComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'dashboard' }
];
