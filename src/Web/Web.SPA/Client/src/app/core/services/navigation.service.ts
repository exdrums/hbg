import { Injectable } from '@angular/core';
import { Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { PermissionService } from './permission.service';
import { AuthService } from './auth.service';
import { navigation, NavigationItem } from '../app-navigation';

/**
 * Service for managing application navigation with permission-based filtering
 */
@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  constructor(
    private permissionService: PermissionService,
    private authService: AuthService
  ) {}

  /**
   * Get filtered navigation items based on user permissions
   */
  public getNavigationItems$(): Observable<NavigationItem[]> {
    return combineLatest([
      this.authService.isAuthenticated$,
      this.permissionService.scopes$
    ]).pipe(
      map(([isAuthenticated, scopes]) => {
        if (!isAuthenticated) {
          return [];
        }
        return this.filterNavigationItems(navigation);
      })
    );
  }

  /**
   * Get navigation items synchronously (for immediate use)
   */
  public getNavigationItems(): NavigationItem[] {
    if (!this.authService.isAuthenticated()) {
      return [];
    }
    return this.filterNavigationItems(navigation);
  }

  /**
   * Filter navigation items based on user permissions
   */
  private filterNavigationItems(items: NavigationItem[]): NavigationItem[] {
    return items
      .filter(item => this.canAccessItem(item))
      .map(item => {
        // If item has children, filter them recursively
        if (item.items && item.items.length > 0) {
          const filteredChildren = this.filterNavigationItems(item.items);
          return {
            ...item,
            items: filteredChildren
          };
        }
        return item;
      })
      // Remove parent items that have no accessible children
      .filter(item => {
        if (item.items && item.items.length === 0 && !item.path) {
          return false; // Parent with no children and no direct path
        }
        return true;
      });
  }

  /**
   * Check if user can access a navigation item based on required scopes
   */
  private canAccessItem(item: NavigationItem): boolean {
    // If no scopes required, item is accessible to all authenticated users
    if (!item.requiredScopes || item.requiredScopes.length === 0) {
      return true;
    }

    // Check if user has any of the required scopes
    return this.permissionService.hasAnyScope(item.requiredScopes);
  }

  /**
   * Check if a specific path is accessible to the current user
   */
  public canAccessPath(path: string): boolean {
    const item = this.findNavigationItemByPath(navigation, path);
    if (!item) {
      return false; // Path not found in navigation
    }
    return this.canAccessItem(item);
  }

  /**
   * Find a navigation item by its path
   */
  private findNavigationItemByPath(items: NavigationItem[], path: string): NavigationItem | null {
    for (const item of items) {
      if (item.path === path) {
        return item;
      }
      if (item.items && item.items.length > 0) {
        const found = this.findNavigationItemByPath(item.items, path);
        if (found) {
          return found;
        }
      }
    }
    return null;
  }
}
