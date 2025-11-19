import { Injectable } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

/**
 * Service for checking user permissions based on OAuth scopes
 */
@Injectable({
  providedIn: 'root'
})
export class PermissionService {
  private scopesSubject = new BehaviorSubject<string[]>([]);
  public scopes$ = this.scopesSubject.asObservable();

  constructor(private oauthService: OAuthService) {
    this.updateScopes();

    // Update scopes when token changes
    this.oauthService.events.subscribe(() => {
      this.updateScopes();
    });
  }

  /**
   * Update the current user's scopes from the access token
   */
  private updateScopes(): void {
    const claims = this.oauthService.getIdentityClaims() as any;
    if (claims && claims.scope) {
      // Scopes can be a string or array
      const scopes = typeof claims.scope === 'string'
        ? claims.scope.split(' ')
        : claims.scope;
      this.scopesSubject.next(scopes);
    } else {
      // Fallback: try to get scopes from access token
      const token = this.oauthService.getAccessToken();
      if (token) {
        try {
          const payload = this.parseJwt(token);
          if (payload && payload.scope) {
            const scopes = typeof payload.scope === 'string'
              ? payload.scope.split(' ')
              : payload.scope;
            this.scopesSubject.next(scopes);
          } else {
            this.scopesSubject.next([]);
          }
        } catch (e) {
          console.error('Error parsing token:', e);
          this.scopesSubject.next([]);
        }
      } else {
        this.scopesSubject.next([]);
      }
    }
  }

  /**
   * Parse JWT token to extract payload
   */
  private parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (e) {
      return null;
    }
  }

  /**
   * Get current user scopes synchronously
   */
  public getScopes(): string[] {
    return this.scopesSubject.value;
  }

  /**
   * Check if user has a specific scope
   * @param scope The scope to check (e.g., 'api_constructor')
   */
  public hasScope(scope: string): boolean {
    const scopes = this.getScopes();
    return scopes.includes(scope);
  }

  /**
   * Check if user has any of the specified scopes
   * @param scopes Array of scopes to check
   */
  public hasAnyScope(scopes: string[]): boolean {
    if (!scopes || scopes.length === 0) {
      return true; // No scope required
    }
    const userScopes = this.getScopes();
    return scopes.some(scope => userScopes.includes(scope));
  }

  /**
   * Check if user has all of the specified scopes
   * @param scopes Array of scopes to check
   */
  public hasAllScopes(scopes: string[]): boolean {
    if (!scopes || scopes.length === 0) {
      return true; // No scope required
    }
    const userScopes = this.getScopes();
    return scopes.every(scope => userScopes.includes(scope));
  }

  /**
   * Observable that emits true if user has the specified scope
   * @param scope The scope to check
   */
  public hasScope$(scope: string): Observable<boolean> {
    return this.scopes$.pipe(
      map(scopes => scopes.includes(scope))
    );
  }

  /**
   * Observable that emits true if user has any of the specified scopes
   * @param scopes Array of scopes to check
   */
  public hasAnyScope$(scopes: string[]): Observable<boolean> {
    if (!scopes || scopes.length === 0) {
      return new BehaviorSubject(true).asObservable();
    }
    return this.scopes$.pipe(
      map(userScopes => scopes.some(scope => userScopes.includes(scope)))
    );
  }
}
