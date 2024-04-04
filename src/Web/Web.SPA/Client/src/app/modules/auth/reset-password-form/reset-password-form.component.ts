import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@app/core/services/auth.service';


const notificationText = 'We\'ve sent a link to reset your password. Check your inbox.';

@Component({
  selector: 'hbg-reset-password-form',
  templateUrl: './reset-password-form.component.html',
  styleUrls: ['./reset-password-form.component.scss'],
})
export class ResetPasswordFormComponent {
  @Input() signInLink = '/auth/login';
  @Input() buttonLink = '/auth/login';

  loading = false;

  formData: any = {};

  constructor(private authService: AuthService, private router: Router) { }

  async onSubmit(e: Event) {
    e.preventDefault();
    const { email } = this.formData;
    this.loading = true;

    const result = await this.authService.resetPassword(email);
    this.loading = false;

    // if (result.isOk) {
    //   this.router.navigate([this.buttonLink]);
    //   notify(notificationText, 'success', 2500);
    // } else {
    //   notify(result.message, 'error', 2000);
    // }
  }
}
