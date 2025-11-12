import { Routes } from '@angular/router';
import { AuthGuard } from '@app/core/guards/auth.guard';
import { ConstructorMainComponent } from './components/constructor-main/constructor-main.component';
import { ProjectListComponent } from './components/project-list/project-list.component';

export const constructorRoutes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
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
