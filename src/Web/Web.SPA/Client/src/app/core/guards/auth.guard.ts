import { CanActivate, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Injectable } from "@angular/core";
import { AuthService } from "../services/auth.service";

@Injectable()
export class AuthGuard implements CanActivate {
    constructor(private router: Router, private authService: AuthService) {}

    canActivate(route: ActivatedRouteSnapshot): boolean {
        // console.log('AuthGuard');
        const isLoggedIn = this.authService.authStatus;
        // console.log('AuthGuard_isLoggedIn', isLoggedIn);
        const isLoginForm = route.routeConfig.path === 'auth/login';

        if (isLoggedIn && isLoginForm) {
            // console.log('isLoggedIn && isLoginForm');
            this.router.navigate(['/']);
            return false;
        }

        if (!isLoggedIn && !isLoginForm) {
            // console.log('!isLoggedIn && !isLoginForm', !isLoggedIn, !isLoginForm);
            this.router.navigate(['/auth/login']);
            return false;
        }
        // console.log('Return isLoggedIn || isLoginForm', isLoggedIn || isLoginForm);

        return isLoggedIn || isLoginForm;
    }
}