# HBG Identity Services Communication Analysis

## Overview
This document analyzes the communication patterns between the OIDC Authorization Server (IdentityServer4) and Admin Service in the HBG platform, focusing on local development configuration with self-signed certificates.

## Architecture Components

### 1. OIDC Authorization Server (API.Identity)
- **Location**: `/src/Services/API/Identity/API.Identity`
- **K8s Service**: `hbg-sts-service`
- **Domain**: `sts.hbg.local`
- **Port**: 80 (internal), HTTPS via ingress
- **Function**: OAuth2/OIDC authorization server using IdentityServer4

### 2. Admin Service (API.Identity.Admin)
- **Location**: `/src/Services/API/Identity/API.Identity.Admin`
- **K8s Service**: `hbg-admin-service`
- **Domain**: `admin.hbg.local`
- **Port**: 80 (internal), HTTPS via ingress
- **Function**: Administrative UI for managing OIDC server

## Communication Patterns

### 1. Direct HTTP Communication
The Admin Service communicates with OIDC Server via direct HTTP requests:

**Configuration Sources (K8s ConfigMap)**:
```yaml
hbg-sts-url: https://sts.hbg.local
hbg-admin-url: https://admin.hbg.local
hbg-admin-url-login: https://admin.hbg.local/signin-oidc
hbg-admin-url-logout: https://admin.hbg.local/signout-callback-oidc
```

**Admin Service Environment Variables**:
- `AdminConfiguration__IdentityServerBaseUrl` → `hbg-sts-url`
- `AdminConfiguration__IdentityAdminBaseUrl` → `hbg-admin-url`
- `AdminConfiguration__IdentityAdminRedirectUri` → `hbg-admin-url-login`

### 2. OIDC Authentication Flow
The Admin Service acts as an OIDC client to the Authorization Server:

**Client Configuration** (from `identityserverdata.json`):
```json
{
  "ClientId": "client_admin",
  "ClientName": "client_admin",
  "ClientUri": "https://admin.houbirg.local",
  "AllowedGrantTypes": ["authorization_code"],
  "RequirePkce": true,
  "ClientSecrets": [{"Value": "AdminClientSecret"}],
  "RedirectUris": [
    "http://localhost:5798/signin-oidc",
    "https://localhost:5798/signin-oidc", 
    "https://admin.houbirg.local/signin-oidc"
  ]
}
```

**Admin Service OIDC Configuration**:
```json
{
  "ClientId": "client_admin",
  "ClientSecret": "AdminClientSecret",
  "OidcResponseType": "code",
  "Scopes": ["openid", "profile", "email", "roles"],
  "RequireHttpsMetadata": false
}
```

## Certificate Management

### Self-Signed CA Setup
The system uses cert-manager to create a self-signed CA for local development:

1. **Self-Signed Issuer**: Creates the initial CA
2. **CA Certificate**: `hbg-selfsigned-ca` (10-year validity)
   - Organization: HBG GmbH
   - OU: LocalDeployment
   - Algorithm: ECDSA 256-bit
3. **CA ClusterIssuer**: `hbg-ca-issuer` uses the CA secret
4. **Service Certificate**: `hbg-cert` covers all domains:
   - `sts.hbg.local`
   - `admin.hbg.local`  
   - `spa.hbg.local`

### TLS Configuration
**Ingress Configuration**:
```yaml
spec:
  tls:
  - hosts:
    - sts.hbg.local
    - admin.hbg.local
    - spa.hbg.local
    secretName: hbg-tls-secret
```

**Trust Manager Integration**:
- Both services use `trust-manager/inject: "true"` annotation
- CA certificates are automatically injected into containers
- Docker containers update CA certificates via `DockerConfiguration__UpdateCaCertificate: "true"`

## Local Development Configuration

### Prerequisites for Mac
1. **Kubernetes**: Docker Desktop with Kubernetes enabled
2. **cert-manager**: Installed in `cert-manager` namespace
3. **NGINX Ingress**: For routing HTTPS traffic
4. **Hosts File**: `/etc/hosts` entries:
   ```
   127.0.0.1   sts.hbg.local admin.hbg.local spa.hbg.local
   ```
5. **CA Trust**: Import `hbg-selfsigned-ca.crt` into macOS Keychain as trusted

### Environment Variables (Both Services)
```yaml
env:
- name: ASPNETCORE_ENVIRONMENT
  value: Development
- name: DockerConfiguration__UpdateCaCertificate
  value: "true"
```

### Database Configuration
Both services share the same PostgreSQL database:
```yaml
ConnectionStrings__ConfigurationDbConnection: (from secret)
ConnectionStrings__PersistedGrantDbConnection: (from secret)  
ConnectionStrings__IdentityDbConnection: (from secret)
ConnectionStrings__DataProtectionDbConnection: (from secret)
```

## Configuration Synchronization Issues

### Domain Mismatches
**Problem**: Configuration files contain mixed domain references:
- K8s ConfigMap uses: `*.hbg.local`
- JSON configs use: `*.houbirg.local`
- Some redirect URIs reference localhost ports

**Example from `identityserverdata.json`**:
```json
"ClientUri": "https://admin.houbirg.local",
"RedirectUris": [
  "http://localhost:5798/signin-oidc",
  "https://localhost:5798/signin-oidc",
  "https://admin.houbirg.local/signin-oidc"  // Should be admin.hbg.local
]
```

### Port Configuration Inconsistencies
**Local Development Ports** (from appsettings.json):
- Admin Service: `localhost:5798`
- OIDC Server: `localhost:5700`

**K8s Service Discovery**:
- Internal communication via service names
- External access via ingress on port 443

## Recommendations for Local Development

### 1. Standardize Domain Names
- Use consistent domain pattern: `*.hbg.local`
- Update all configuration files to match K8s ConfigMap
- Remove references to `houbirg.local`

### 2. Environment-Specific Configuration
- Separate local development from K8s configuration
- Use environment variables for domain overrides
- Implement configuration validation

### 3. Certificate Trust Automation
- Automate CA certificate extraction and trust installation
- Create setup scripts for local development
- Document certificate renewal process

### 4. Service Health Monitoring
- Implement proper health checks for certificate validity
- Monitor OIDC flows between services
- Add logging for certificate trust issues

## Security Considerations

### Development vs Production
- **Development**: Self-signed certificates, `RequireHttpsMetadata: false`
- **Production**: Should use proper CA-signed certificates
- **Secret Management**: Use Kubernetes secrets for sensitive data

### CORS Configuration
Both services configure CORS for cross-origin requests:
```json
"AllowedCorsOrigins": [
  "http://localhost:5798",
  "https://localhost:5798", 
  "https://admin.hbg.local"
]
```

### Client Authentication
- Admin Service uses Authorization Code flow with PKCE
- Client secrets stored in configuration (should use secrets in production)
- Token validation configured for `name` and `role` claims

## Troubleshooting Guide

### Common Issues
1. **Certificate Trust**: Ensure CA is trusted in macOS Keychain
2. **Domain Resolution**: Verify `/etc/hosts` entries
3. **Service Discovery**: Check K8s service endpoints
4. **OIDC Flow**: Validate redirect URIs match exactly
5. **CORS**: Ensure allowed origins include all necessary domains

### Diagnostic Commands
```bash
# Check certificate status
kubectl describe certificate hbg-cert

# Extract CA certificate
kubectl get secret hbg-selfsigned-ca-secret -n cert-manager -o jsonpath="{.data['tls\.crt']}" | base64 --decode

# Test service connectivity
kubectl port-forward svc/hbg-sts-service 5700:80
kubectl port-forward svc/hbg-admin-service 5798:80
```