import { Routes } from '@angular/router';
import { authenticationGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    // Rutas públicas
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login').then(m => m.Login)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register/register').then(m => m.Register)
  },

  // Rutas privadas — solo pasan si hay sesión
  {
    path: 'dashboard',
    canActivate: [authenticationGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard').then(m => m.Dashboard)
  },

  // Redirecciones
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'dashboard' }
];
