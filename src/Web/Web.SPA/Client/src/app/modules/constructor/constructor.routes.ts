import { Routes } from '@angular/router';
import { AuthGuard } from '@app/core/guards/auth.guard';
import { PermissionGuard } from '@app/core/guards/permission.guard';
import { ConstructorMainComponent } from './components/constructor-main/constructor-main.component';
import { ProjectListComponent } from './components/project-list/project-list.component';

export const constructorRoutes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard, PermissionGuard],
    data: { requiredScopes: ['api_constructor'] },
    children: [
      {
        path: '',
        component: ProjectListComponent
      },
      {
        path: ':id',
        component: ConstructorMainComponent
      }
    ]
  }
];
