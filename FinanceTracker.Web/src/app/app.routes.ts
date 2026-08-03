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
    path: '',
    loadComponent: () =>
      import('./layout/main-layout/main-layout').then(m => m.MainLayout),
    canActivate: [authenticationGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard').then(m => m.Dashboard),
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./features/categories/categories').then(m => m.Categories),
      },
      {
        path: 'accounts',
        loadComponent: () =>
          import('./features/accounts/accounts').then(m => m.Accounts),
      },
      {
        path: 'transactions',
        loadComponent: () =>
          import('./features/transactions/transactions').then(m => m.Transactions),
      },
    ],
  },

  // Redirecciones
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'dashboard' }
];
