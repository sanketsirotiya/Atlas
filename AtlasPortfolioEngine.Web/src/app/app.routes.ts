import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'clients',
    loadComponent: () =>
      import('./features/clients/client-list/client-list').then((m) => m.ClientList),
    canActivate: [authGuard],
  },
  {
    path: 'clients/:id',
    loadComponent: () =>
      import('./features/clients/client-detail/client-detail').then((m) => m.ClientDetail),
    canActivate: [authGuard],
  },
  {
    path: 'clients/:id/portfolio',
    loadComponent: () =>
      import('./features/portfolio/portfolio-dashboard/portfolio-dashboard').then(
        (m) => m.PortfolioDashboard,
      ),
    canActivate: [authGuard],
  },
  {
    path: 'clients/:id/drift',
    loadComponent: () =>
      import('./features/portfolio/drift-report/drift-report').then((m) => m.DriftReport),
    canActivate: [authGuard],
  },
  {
    path: 'clients/:id/rebalance',
    loadComponent: () =>
      import('./features/rebalance/rebalance-preview/rebalance-preview').then(
        (m) => m.RebalancePreview,
      ),
    canActivate: [authGuard],
  },
  {
    path: 'clients/:id/suitability',
    loadComponent: () =>
      import('./features/suitability/suitability-check/suitability-check').then(
        (m) => m.SuitabilityCheck,
      ),
    canActivate: [authGuard],
  },
  { path: '**', redirectTo: 'login' },
];
