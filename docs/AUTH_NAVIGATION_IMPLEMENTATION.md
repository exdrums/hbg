# Permission-Based Navigation and Constructor Integration

## Overview

This document describes the implementation of permission-based navigation in the Web.SPA application, including the integration of the Constructor service with proper authentication and authorization controls.

## Implementation Summary

The system now supports role-based access control (RBAC) using OAuth2 scopes. Navigation items and routes are automatically filtered based on the user's granted API scopes, ensuring users only see and can access features they have permission to use.

## Key Features

1. **Permission Service** - Checks user scopes/permissions from OAuth tokens
2. **Navigation Service** - Filters navigation items based on user permissions
3. **Permission Guard** - Protects routes from unauthorized access
4. **Dynamic Sidebar** - Shows only authorized navigation items
5. **Constructor Integration** - Adds Constructor module with `api_constructor` scope requirement

## Architecture

### 1. Permission Service
**Location:** `src/Web/Web.SPA/Client/src/app/core/services/permission.service.ts`

The `PermissionService` is responsible for:
- Extracting scopes from OAuth access tokens
- Providing methods to check if a user has specific scopes
- Exposing observable streams for reactive permission checking

**Key Methods:**
```typescript
hasScope(scope: string): boolean
hasAnyScope(scopes: string[]): boolean
hasAllScopes(scopes: string[]): boolean
hasScope$(scope: string): Observable<boolean>
hasAnyScope$(scopes: string[]): Observable<boolean>
getScopes(): string[]
```

**Usage Example:**
```typescript
// Check if user has constructor access
if (this.permissionService.hasScope('api_constructor')) {
  // Allow access to constructor features
}

// Check if user has any of multiple scopes
if (this.permissionService.hasAnyScope(['api_projects', 'api_emailer'])) {
  // Show combined features
}
```

### 2. Navigation Service
**Location:** `src/Web/Web.SPA/Client/src/app/core/services/navigation.service.ts`

The `NavigationService` is responsible for:
- Filtering navigation items based on user permissions
- Providing both observable and synchronous access to filtered navigation
- Recursively filtering nested navigation items

**Key Methods:**
```typescript
getNavigationItems$(): Observable<NavigationItem[]>
getNavigationItems(): NavigationItem[]
canAccessPath(path: string): boolean
```

**Usage Example:**
```typescript
// Get filtered navigation items
this.navigationService.getNavigationItems$().subscribe(items => {
  this.navigationItems = items;
});
```

### 3. Permission Guard
**Location:** `src/Web/Web.SPA/Client/src/app/core/guards/permission.guard.ts`

The `PermissionGuard` protects routes from unauthorized access by:
- Checking if the user is authenticated
- Verifying the user has required scopes from route data
- Redirecting unauthorized users to the home page

**Usage Example:**
```typescript
// In routing configuration
{
  path: 'constructor',
  component: ConstructorComponent,
  canActivate: [AuthGuard, PermissionGuard],
  data: { requiredScopes: ['api_constructor'] }
}
```

### 4. Navigation Configuration
**Location:** `src/Web/Web.SPA/Client/src/app/core/app-navigation.ts`

Navigation items now support a `requiredScopes` property:

```typescript
export interface NavigationItem {
  text: string;
  path?: string;
  icon?: string;
  items?: NavigationItem[];
  expanded?: boolean;
  requiredScopes?: string[]; // API scopes required to access this item
}

export const navigation: NavigationItem[] = [
  {
    text: 'Home',
    path: '/home',
    icon: 'home',
    // No required scopes - accessible to all authenticated users
  },
  {
    text: 'Projects',
    path: '/projects',
    icon: 'product',
    requiredScopes: ['api_projects']
  },
  {
    text: 'Constructor',
    path: '/constructor',
    icon: 'toolbox',
    requiredScopes: ['api_constructor']
  }
];
```

### 5. Side Navigation Menu Component
**Location:** `src/Web/Web.SPA/Client/src/app/modules/shell/side-navigation-menu/side-navigation-menu.component.ts`

Updated to use `NavigationService` for filtering:
- Subscribes to filtered navigation items on initialization
- Automatically updates when user permissions change
- Properly cleans up subscriptions on destroy

## OAuth Scopes

### Available API Scopes

The following scopes are configured in the authentication service:

| Scope | Description | Module |
|-------|-------------|--------|
| `openid` | OpenID Connect identity | Core |
| `profile` | User profile information | Core |
| `email` | User email address | Core |
| `roles` | User roles | Core |
| `offline_access` | Refresh token support | Core |
| `api_files` | Files API access | Files Module |
| `api_projects` | Projects API access | Projects Module |
| `api_emailer` | Emailer API access | Emailer Module |
| `api_contacts` | Contacts API access | Contacts Module |
| `api_constructor` | Constructor API access | Constructor Module |

### Scope Configuration

**Client-side (Auth Service):**
`src/Web/Web.SPA/Client/src/app/core/services/auth.service.ts:39`
```typescript
scope: "openid profile email roles offline_access api_files api_projects api_emailer api_contacts api_constructor"
```

**Allowed URLs for API calls:**
```typescript
this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgidentity);
this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgfiles);
this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgprojects);
this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgemailer);
this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgconstructor);
```

## Constructor Service Integration

### Configuration Updates

1. **App Settings Model**
   - `src/Web/Web.SPA/Client/src/app/core/models/app-settings.model.ts:10`
   - Added `hbgconstructor: string` property
   - Default value: `http://localhost:5705`

2. **Config Service**
   - `src/Web/Web.SPA/Client/src/app/core/services/config.service.ts:16`
   - Added `hbgconstructor: string` property

3. **Backend Configuration**
   - `src/Web/Web.SPA/appsettings.json:13`
   - `src/Web/Web.SPA/appsettings.Development.json:15`
   - Added `HBGCONSTRUCTOR: "http://localhost:5705"`

### Constructor Routes

**Location:** `src/Web/Web.SPA/Client/src/app/modules/constructor/constructor.routes.ts`

```typescript
export const constructorRoutes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard, PermissionGuard],
    data: { requiredScopes: ['api_constructor'] },
    children: [
      {
        path: '',
        component: ProjectListComponent
      },
      {
        path: ':id',
        component: ConstructorMainComponent
      }
    ]
  }
];
```

### Main App Routes

**Location:** `src/Web/Web.SPA/Client/src/app/app.routes.ts`

Updated routes with permission guards:

```typescript
{
  path: "projects",
  component: ProjectsComponent,
  canActivate: [PermissionGuard],
  data: { requiredScopes: ['api_projects'] }
},
{
  path: "emailer",
  component: EmailerComponent,
  canActivate: [PermissionGuard],
  data: { requiredScopes: ['api_emailer'] }
},
{
  path: "constructor",
  loadChildren: () => import('./modules/constructor/constructor.module').then(m => m.ConstructorModule)
}
```

## How It Works

### User Login Flow

1. User authenticates via OAuth2/OIDC
2. Identity provider issues access token with granted scopes
3. `AuthService` stores the token and triggers authentication events
4. `PermissionService` extracts scopes from the token
5. `NavigationService` filters navigation items based on scopes
6. `SideNavigationMenuComponent` displays only authorized items

### Navigation Access Control

1. User attempts to navigate to a route
2. `AuthGuard` checks if user is authenticated
3. `PermissionGuard` checks if user has required scopes
4. If authorized: navigation proceeds
5. If unauthorized: user is redirected to `/home`

### Dynamic Sidebar Updates

1. `SideNavigationMenuComponent` subscribes to `NavigationService.getNavigationItems$()`
2. When user logs in or permissions change, the service emits new filtered items
3. Component updates the DevExtreme TreeView with authorized items only
4. Users only see menu items they can access

## Testing

### Verify Permission Service

```typescript
// In browser console after login
const permissionService = // inject PermissionService
console.log('User scopes:', permissionService.getScopes());
console.log('Has api_constructor:', permissionService.hasScope('api_constructor'));
```

### Verify Navigation Filtering

1. Log in with a user that has only `api_projects` scope
2. Verify that only Home, Projects, and Chat appear in sidebar
3. Log in with a user that has `api_constructor` scope
4. Verify that Constructor item appears in sidebar

### Verify Route Protection

1. Try to navigate directly to `/constructor` without `api_constructor` scope
2. Verify redirect to `/home`
3. Check console for permission denied messages

## Identity Server Configuration

To grant scopes to users, configure the Identity Server:

1. **API Resources**: Define API resources for each scope
   - api_files
   - api_projects
   - api_emailer
   - api_contacts
   - api_constructor

2. **Client Configuration**: Update the "js" client to include new scopes in `AllowedScopes`

3. **User Claims**: Ensure users have appropriate claims/roles that map to scopes

## Future Enhancements

1. **Fine-grained Permissions**: Support operation-level permissions (read, write, delete)
2. **Permission Caching**: Cache permission checks for better performance
3. **Admin UI**: Create admin interface for managing user permissions
4. **Audit Logging**: Log permission checks and access attempts
5. **Dynamic Scope Loading**: Load scopes dynamically from backend configuration

## Troubleshooting

### Navigation Items Not Filtering

1. Check browser console for errors
2. Verify `PermissionService` is extracting scopes correctly
3. Check that navigation items have correct `requiredScopes` configuration
4. Ensure `NavigationService` is injected in `SideNavigationMenuComponent`

### Routes Not Protected

1. Verify `PermissionGuard` is imported in routes
2. Check that routes have `data: { requiredScopes: [...] }` configuration
3. Ensure `PermissionGuard` is added to `canActivate` array

### Scopes Not Present in Token

1. Check Identity Server client configuration
2. Verify user has appropriate roles/claims
3. Check Auth Service scope request string
4. Inspect access token JWT payload

## Files Modified

### New Files Created
- `src/Web/Web.SPA/Client/src/app/core/services/permission.service.ts`
- `src/Web/Web.SPA/Client/src/app/core/services/navigation.service.ts`
- `src/Web/Web.SPA/Client/src/app/core/guards/permission.guard.ts`

### Files Modified
- `src/Web/Web.SPA/Client/src/app/core/app-navigation.ts`
- `src/Web/Web.SPA/Client/src/app/core/services/auth.service.ts`
- `src/Web/Web.SPA/Client/src/app/modules/shell/side-navigation-menu/side-navigation-menu.component.ts`
- `src/Web/Web.SPA/Client/src/app/modules/constructor/constructor.routes.ts`
- `src/Web/Web.SPA/Client/src/app/app.routes.ts`
- `src/Web/Web.SPA/appsettings.json`
- `src/Web/Web.SPA/appsettings.Development.json`

## Conclusion

The permission-based navigation system provides a robust, maintainable way to control feature access in the Web.SPA application. By leveraging OAuth scopes and Angular guards, the system ensures that users only see and can access features they're authorized to use, improving both security and user experience.
