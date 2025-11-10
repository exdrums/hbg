import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { NoAuthGuard } from './core/guards/no-auth.guard';
import { ShellComponent } from './modules/shell/shell/shell.component';

export const routes: Routes = [
  {
    path: 'auth',
    canActivate: [NoAuthGuard],
    children: [
      {
        path: 'login',
        loadChildren: () => import('./modules/auth/auth.module').then(m => m.AuthModule)
      }
    ]
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadChildren: () => import('./modules/dashboard/dashboard.module').then(m => m.DashboardModule)
      },
      {
        path: 'clients',
        loadChildren: () => import('./modules/clients/clients.module').then(m => m.ClientsModule)
      },
      {
        path: 'api-resources',
        loadChildren: () => import('./modules/api-resources/api-resources.module').then(m => m.ApiResourcesModule)
      },
      {
        path: 'api-scopes',
        loadChildren: () => import('./modules/api-scopes/api-scopes.module').then(m => m.ApiScopesModule)
      },
      {
        path: 'identity-resources',
        loadChildren: () => import('./modules/identity-resources/identity-resources.module').then(m => m.IdentityResourcesModule)
      }
      // TODO: Uncomment as modules are implemented
      // {
      //   path: 'users',
      //   loadChildren: () => import('./modules/users/users.module').then(m => m.UsersModule)
      // },
      // {
      //   path: 'roles',
      //   loadChildren: () => import('./modules/roles/roles.module').then(m => m.RolesModule)
      // },
      // {
      //   path: 'grants',
      //   loadChildren: () => import('./modules/grants/grants.module').then(m => m.GrantsModule)
      // }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
