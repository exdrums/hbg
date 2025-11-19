import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { PermissionService } from '../services/permission.service';
import { AuthService } from '../services/auth.service';

/**
 * Route guard that checks if user has required permissions/scopes
 * Usage in routes:
 * {
 *   path: 'constructor',
 *   canActivate: [PermissionGuard],
 *   data: { requiredScopes: ['api_constructor'] }
 * }
 */
@Injectable({
  providedIn: 'root'
})
export class PermissionGuard implements CanActivate {
  constructor(
    private permissionService: PermissionService,
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    // Check if user is authenticated
    if (!this.authService.isAuthenticated()) {
      console.warn('PermissionGuard: User not authenticated, redirecting to home');
      return this.router.createUrlTree(['/home']);
    }

    // Get required scopes from route data
    const requiredScopes = route.data['requiredScopes'] as string[] | undefined;

    // If no scopes required, allow access
    if (!requiredScopes || requiredScopes.length === 0) {
      return true;
    }

    // Check if user has any of the required scopes
    const hasPermission = this.permissionService.hasAnyScope(requiredScopes);

    if (!hasPermission) {
      console.warn(`PermissionGuard: Access denied. Required scopes: ${requiredScopes.join(', ')}`);
      console.warn(`PermissionGuard: User scopes: ${this.permissionService.getScopes().join(', ')}`);
      // Redirect to home if user doesn't have permission
      return this.router.createUrlTree(['/home']);
    }

    return true;
  }
}
