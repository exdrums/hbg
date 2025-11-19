import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { HomeComponent } from './modules/home/home.component';
import { NoAuthGuard } from './core/guards/no-auth.guard';
import { AuthComponent } from './modules/auth/auth.component';
import { LoginFormComponent } from './modules/auth/login-form/login-form.component';
import { ResetPasswordFormComponent } from './modules/auth/reset-password-form/reset-password-form.component';
import { CreateAccountFormComponent } from './modules/auth/create-account-form/create-account-form.component';
import { ChangePasswordFormComponent } from './modules/auth/change-password-form/change-password-form.component';
import { ShellComponent } from './modules/shell/shell.component';
import { ProjectsComponent } from './modules/projects/projects.component';
import { EmailerComponent } from './modules/emailer/emailer.component';
import { MainChatComponent } from './modules/chat/components/main-chat/main-chat.component';

export const routes: Routes = [
  {
    path: 'auth',
    component: AuthComponent,
    children: [
      {
        path: 'login',
        component: LoginFormComponent,
        canActivate: [NoAuthGuard],
      },
      {
        path: 'reset-password',
        component: ResetPasswordFormComponent,
        canActivate: [NoAuthGuard],
      },
      {
        path: 'create-account',
        component: CreateAccountFormComponent,
        canActivate: [NoAuthGuard],
      },
      {
        path: 'change-password/:recoveryCode',
        component: ChangePasswordFormComponent,
        canActivate: [NoAuthGuard],
      },
      {
        path: '**',
        redirectTo: 'login',
        pathMatch: 'full',
      },
    ]
  },
  {
    path: "",
    component: ShellComponent,
    runGuardsAndResolvers: "always",
    children: [
      {
        path: "",
        redirectTo: "/home",
        pathMatch: "full"
      },
      {
        path: 'home',
        component: HomeComponent,
      },
      {
        path: "projects",
        component: ProjectsComponent
      },
      {
        path: "emailer",
        component: EmailerComponent
      },
      {
        path: "chat",
        component: MainChatComponent
      },
      {
        path: "constructor",
        // @ts-ignore
        loadChildren: () => import('./modules/constructor/constructor.module').then(m => m.ConstructorModule)
      }
      // {
      //   path: 'scheduler',
      //   component: SchedulerComponent
      // }
    ],
    canActivate: [ AuthGuard ]
  }
];
