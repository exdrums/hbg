import { Injectable } from "@angular/core";
import { Router, CanActivate, ActivatedRouteSnapshot } from "@angular/router";
import { Observable } from "rxjs";
import { AuthService } from "../services/auth.service";


@Injectable()
export class NoAuthGuard implements CanActivate {
	constructor(private authService: AuthService, private router: Router) {}

	canActivate(
		next: ActivatedRouteSnapshot
	): Observable<boolean> | Promise<boolean> | boolean {
		// console.log('NoAuthGuard');
		if (this.authService.authStatus == false) {
            // console.log('this.authService.authStatus == false');
			return true;
		}
        // this.toast.message("You are already logged in.");
        // console.log("You are already logged in.");
		// this.router.navigate(["/home"]);
		// return false;
		return true;
	}
}
