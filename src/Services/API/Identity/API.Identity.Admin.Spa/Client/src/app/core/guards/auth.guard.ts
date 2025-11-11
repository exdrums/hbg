import { CanActivate, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";
import { AuthService } from "../services/auth.service";

@Injectable()
export class AuthGuard implements CanActivate {
  constructor(private router: Router, private authService: AuthService) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const isLoggedIn = this.authService.authStatus;
    const isLoginForm = route.routeConfig.path === 'auth/login';

    if (isLoggedIn && isLoginForm) {
      this.router.navigate(['/']);
      return false;
    }

    if (!isLoggedIn && !isLoginForm) {
      this.router.navigate(['/auth/login']);
      return false;
    }

    return isLoggedIn || isLoginForm;
  }
}
