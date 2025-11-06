# API.Identity.Admin.Spa

An Angular SPA application with DevExtreme components for administering the Identity Server (OIDC/OAuth2).

## Overview

This project provides a modern, single-page application (SPA) interface for managing all aspects of the Identity Server, including:

- **Clients** - OAuth2/OIDC client applications
- **Users** - User accounts and profiles
- **Roles** - User roles and permissions
- **API Resources** - Protected API resources
- **API Scopes** - Fine-grained API access scopes
- **Identity Resources** - User identity claims (OpenID Connect scopes)
- **Persisted Grants** - Authorization codes, tokens, and consent grants

## Architecture

### Backend (ASP.NET Core 9.0)
- **Purpose**: Hosts the Angular SPA and provides configuration endpoint
- **Port**: 5796 (HTTPS), 4201 (dev)
- **Configuration Endpoint**: `/configuration` - Provides API URLs to Angular app
- **Health Checks**: `/health`, `/hc` (UI), `/hc-api` (API)

### Frontend (Angular 18.2.12)
- **UI Framework**: DevExtreme 24.2.5
- **Authentication**: angular-oauth2-oidc 15.0.1 (OAuth2 password flow)
- **State Management**: RxJS 7.8.1
- **Build System**: Angular CLI with AOT compilation

## Project Structure

```
API.Identity.Admin.Spa/
├── API.Identity.Admin.Spa.csproj          # ASP.NET Core project
├── Program.cs                              # Application entry point
├── Startup.cs                              # Service configuration
├── Controllers/
│   └── ConfigurationController.cs          # Configuration API
├── appsettings.json                        # Production settings
├── appsettings.Development.json            # Development settings
└── Client/                                 # Angular application
    ├── angular.json                        # Angular CLI configuration
    ├── package.json                        # Node dependencies
    ├── tsconfig.json                       # TypeScript configuration
    └── src/
        ├── main.ts                         # Bootstrap entry
        ├── index.html                      # HTML shell
        ├── styles/                         # Global styles & themes
        ├── environments/                   # Environment configs
        └── app/
            ├── app.module.ts               # Root module
            ├── app.component.ts            # Root component
            ├── app.routes.ts               # Route definitions
            ├── core/                       # Core infrastructure
            │   ├── services/               # Singleton services
            │   │   ├── auth.service.ts     # OIDC authentication
            │   │   ├── config.service.ts   # Dynamic configuration
            │   │   ├── popup.service.ts    # Modal dialogs
            │   │   ├── notification.service.ts  # Notifications
            │   │   ├── theme.service.ts    # Theme switching
            │   │   └── screen-locker.service.ts  # Loading states
            │   ├── guards/                 # Route guards
            │   │   ├── auth.guard.ts       # Authenticated routes
            │   │   └── no-auth.guard.ts    # Public routes
            │   ├── data/                   # Data layer
            │   │   ├── rest-data-store.ts  # DevExtreme REST store
            │   │   ├── custom-data-source.ts  # Enhanced data source
            │   │   └── options.ts          # Data options/types
            │   ├── errors/                 # Error handling
            │   │   ├── http.interceptor.ts # HTTP error interceptor
            │   │   └── error.handler.ts    # Global error handler
            │   ├── components/             # Core components
            │   │   └── hbg-popup/          # Popup system
            │   ├── devextreme/             # DevExtreme defaults
            │   └── utils/                  # Utility functions
            ├── shared/                     # Shared modules
            └── modules/                    # Feature modules
                ├── auth/                   # Authentication module
                ├── shell/                  # App shell & navigation
                ├── dashboard/              # Dashboard
                ├── clients/                # Client management
                ├── users/                  # User management
                ├── roles/                  # Role management
                ├── api-resources/          # API Resource management
                ├── api-scopes/             # API Scope management
                ├── identity-resources/     # Identity Resource management
                └── grants/                 # Grant management
```

## Technology Stack

### Backend
- **.NET 9.0** - Latest .NET framework
- **Serilog** - Structured logging
- **Health Checks** - Application health monitoring

### Frontend
- **Angular 18.2.12** - Modern web framework
- **DevExtreme 24.2.5** - Professional UI components
  - DxDataGrid - Advanced data grids
  - DxForm - Dynamic forms
  - DxPopup - Modal dialogs
  - DxButton, DxToolbar, etc.
- **angular-oauth2-oidc 15.0.1** - OAuth2/OIDC client
- **RxJS 7.8.1** - Reactive programming
- **TypeScript 5.5.2** - Type-safe JavaScript

## Configuration

### AppSettings (appsettings.json)
```json
{
  "HBGIDENTITYADMINSPA": "https://localhost:5796",
  "HBGIDENTITYADMINSPADEV": "http://localhost:4201",
  "HBGIDENTITY": "https://localhost:5700",
  "HBGIDENTITYADMINAPI": "http://localhost:5797"
}
```

### OIDC Client Configuration
- **Client ID**: `client_admin_spa`
- **Client Secret**: `admin_spa_secret`
- **Grant Type**: OAuth2 Password Flow
- **Scopes**: `openid profile email roles offline_access api_admin`

## Development

### Prerequisites
- .NET 9.0 SDK
- Node.js 18.x or later
- Angular CLI 18.2.12

### Running the Application

#### Development Mode (Angular Dev Server)
```bash
# Navigate to Client directory
cd src/Services/API/Identity/API.Identity.Admin.Spa/Client

# Install dependencies
npm install

# Start Angular dev server (port 4201)
npm start
```

#### Production Mode (ASP.NET Core)
```bash
# Build Angular app
cd Client
npm install
npm run build

# Run ASP.NET Core
cd ..
dotnet run
```

The application will be available at:
- **Production**: https://localhost:5796
- **Development**: http://localhost:4201

## Authentication Flow

1. User navigates to the application
2. `ConfigService` fetches configuration from `/configuration`
3. `AuthService` initializes OIDC discovery from Identity Server
4. User logs in with username/password (OAuth2 password flow)
5. Access token is stored in sessionStorage
6. Token is automatically attached to all API requests to Admin API

## Data Management Pattern

The application uses a layered data architecture:

1. **RestDataStore** - DevExtreme CustomStore wrapper for REST APIs
2. **CustomDataSource** - Enhanced DevExtreme DataSource with observables
3. **Angular Components** - Bind to DataSource via DevExtreme components

### Example Usage
```typescript
// Create data store for clients
const store = new RestDataStore<Client>(httpClient, {
  key: 'id',
  loadUrl: `${adminApiUrl}/api/clients`,
  insertUrl: `${adminApiUrl}/api/clients`,
  updateUrl: `${adminApiUrl}/api/clients/`,
  removeUrl: `${adminApiUrl}/api/clients`
});

// Create data source
const dataSource = new CustomDataSource<Client>(store);

// Use in template
<dx-data-grid [dataSource]="dataSource">
  <dxi-column dataField="clientId"></dxi-column>
  <dxi-column dataField="clientName"></dxi-column>
</dx-data-grid>
```

## Admin API Endpoints

All endpoints are prefixed with the Admin API base URL (`http://localhost:5797/api`)

### Clients
- `GET /clients` - List all clients
- `GET /clients/{id}` - Get client by ID
- `POST /clients` - Create new client
- `PUT /clients` - Update client
- `DELETE /clients/{id}` - Delete client
- `GET /clients/{id}/secrets` - Get client secrets
- `POST /clients/{id}/secrets` - Add client secret
- `DELETE /clients/secrets/{secretId}` - Remove client secret

### Users
- `GET /users` - List all users
- `GET /users/{id}` - Get user by ID
- `POST /users` - Create new user
- `PUT /users` - Update user
- `DELETE /users/{id}` - Delete user
- `GET /users/{id}/roles` - Get user roles
- `POST /users/roles` - Assign role to user
- `DELETE /users/roles` - Remove role from user

### Roles
- `GET /roles` - List all roles
- `GET /roles/{id}` - Get role by ID
- `POST /roles` - Create new role
- `PUT /roles` - Update role
- `DELETE /roles/{id}` - Delete role

### API Resources
- `GET /apiresources` - List API resources
- `GET /apiresources/{id}` - Get API resource
- `POST /apiresources` - Create resource
- `PUT /apiresources` - Update resource
- `DELETE /apiresources/{id}` - Delete resource

### API Scopes
- `GET /apiscopes` - List API scopes
- `GET /apiscopes/{id}` - Get scope
- `POST /apiscopes` - Create scope
- `PUT /apiscopes` - Update scope
- `DELETE /apiscopes/{id}` - Delete scope

### Identity Resources
- `GET /identityresources` - List identity resources
- `GET /identityresources/{id}` - Get resource
- `POST /identityresources` - Create resource
- `PUT /identityresources` - Update resource
- `DELETE /identityresources/{id}` - Delete resource

### Persisted Grants
- `GET /persistedgrants/subjects` - List grant subjects
- `GET /persistedgrants/{id}` - Get specific grant
- `DELETE /persistedgrants/{id}` - Delete grant

## Current Implementation Status

### ✅ Completed
- [x] ASP.NET Core project structure
- [x] Angular application scaffold
- [x] Core module with services (Auth, Config, Popup, Theme, etc.)
- [x] Guards (AuthGuard, NoAuthGuard)
- [x] HTTP interceptor and error handler
- [x] Popup infrastructure (stack, context, error popup)
- [x] DevExtreme data layer (RestDataStore, CustomDataSource)
- [x] DevExtreme default settings
- [x] Base styles and theme configuration
- [x] TypeScript configuration
- [x] Angular build configuration
- [x] Health checks configuration
- [x] Configuration controller
- [x] AppSettings integration

### 🚧 In Progress / TODO
- [ ] Shared DevExtreme module (centralized DX component exports)
- [ ] Authentication module with login form
- [ ] Shell module with navigation menu
- [ ] Dashboard module with statistics
- [ ] Client management module (list, create, edit, delete)
- [ ] User management module
- [ ] Role management module
- [ ] API Resources management module
- [ ] API Scopes management module
- [ ] Identity Resources management module
- [ ] Grants management module
- [ ] Seed data extension (OIDC client configuration)
- [ ] Dockerfile for containerization
- [ ] E2E tests
- [ ] Unit tests

## Next Steps

1. **Create Shared DevExtreme Module**
   - Export all required DevExtreme components
   - Import in feature modules

2. **Implement Authentication Module**
   - Login form with username/password
   - Password recovery UI
   - Account creation UI

3. **Implement Shell Module**
   - Side navigation drawer
   - Header with user menu
   - Theme switcher component
   - Responsive layout

4. **Implement Admin Feature Modules**
   - For each entity (Clients, Users, Roles, etc.):
     - List view with DevExtreme DataGrid
     - Create/Edit form with DevExtreme Form
     - Delete confirmation
     - Inline editing support

5. **Extend Seed Data**
   - Add `client_admin_spa` to `identityserverdata.json`
   - Configure redirect URIs
   - Set appropriate scopes and grant types

6. **Create Dockerfile**
   - Multi-stage build (node + dotnet)
   - Angular production build
   - ASP.NET Core publish
   - Runtime image configuration

## Contributing

When adding new feature modules, follow this pattern:

1. Create module directory under `modules/`
2. Create `*.module.ts` with routing
3. Create list component with DxDataGrid
4. Create form component for create/edit
5. Create data service with RestDataStore
6. Add route in `app.routes.ts`
7. Add navigation menu item in Shell module

## License

This project is part of the HBG Identity Server solution.
