import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthComponent } from './auth.component';
import { DxButtonModule, DxFormModule, DxLoadIndicatorModule, DxScrollViewModule } from 'devextreme-angular';
import { AuthCardComponent } from './auth-card/auth-card.component';
import { RouterModule } from '@angular/router';
import { OauthButtonsComponent } from './oauth-buttons/oauth-buttons.component';
import { ResetPasswordFormComponent } from './reset-password-form/reset-password-form.component';
import { CreateAccountFormComponent } from './create-account-form/create-account-form.component';
import { ChangePasswordFormComponent } from './change-password-form/change-password-form.component';
import { LoginFormComponent } from './login-form/login-form.component';

@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    
    DxScrollViewModule,
    DxFormModule,
    DxLoadIndicatorModule,
    DxButtonModule
  ],
  exports: [
    AuthComponent,
    LoginFormComponent
  ],
  declarations: [
    AuthComponent,
    AuthCardComponent,
    ChangePasswordFormComponent,
    CreateAccountFormComponent,
    LoginFormComponent,
    OauthButtonsComponent,
    ResetPasswordFormComponent
  ]
})
export class AuthModule { }
