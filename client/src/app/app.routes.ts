import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'table' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'table',
    loadComponent: () =>
      import('./features/short-urls-table/short-urls-table.component').then((m) => m.ShortUrlsTableComponent),
  },
  {
    path: 'urls/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/short-url-info/short-url-info.component').then((m) => m.ShortUrlInfoComponent),
  },
  { path: '**', redirectTo: 'table' },
];
