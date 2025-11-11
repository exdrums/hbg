import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '@app/core/services/auth.service';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-login-form',
  templateUrl: './login-form.component.html',
  styleUrls: ['./login-form.component.scss']
})
export class LoginFormComponent {
  loading = false;
  formData: any = {
    username: '',
    password: ''
  };

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  async onSubmit(e: any) {
    e.preventDefault();

    const { username, password } = this.formData;

    if (!username || !password) {
      notify('Please enter username and password', 'error', 2000);
      return;
    }

    this.loading = true;

    try {
      this.authService.login(username, password);
      // AuthService handles navigation to /dashboard on success
    } catch (error) {
      notify('Login failed. Please check your credentials.', 'error', 3000);
      this.loading = false;
    }
  }

  passwordComparison = () => {
    return this.formData.password;
  };
}
