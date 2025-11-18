# Kubernetes Dashboard Configuration

This directory contains all the necessary Kubernetes manifests to deploy the Kubernetes Dashboard for the HBG cluster, accessible via `k8s.hbg.lol`.

## Components

### 1. Namespace (`namespace.yaml`)
Creates the `kubernetes-dashboard` namespace to isolate dashboard resources.

### 2. RBAC Configuration (`rbac.yaml`)
Includes:
- **ServiceAccounts**:
  - `kubernetes-dashboard`: For the dashboard application
  - `kubernetes-dashboard-admin`: Admin account with cluster-wide permissions
- **ClusterRoles and ClusterRoleBindings**: Permissions for dashboard operation and admin access
- **Secrets**: Certificate and CSRF token storage
- **ConfigMap**: Dashboard settings storage

### 3. Deployment (`deployment.yaml`)
Deploys:
- **Kubernetes Dashboard**: Main web interface (v2.7.0)
- **Metrics Scraper**: Collects and provides metrics for the dashboard (v1.0.9)

### 4. Services (`service.yaml`)
Exposes:
- **kubernetes-dashboard**: Dashboard web interface on ports 80 (HTTP) and 443 (HTTPS)
- **dashboard-metrics-scraper**: Metrics collection service on port 8000

### 5. Ingress (`ingress.yaml`)
Configures external access:
- **Host**: `k8s.hbg.lol`
- **TLS**: Automatic certificate via cert-manager (hbg-ca-issuer)
- **Ingress Controller**: nginx
- **Security**: Rate limiting, security headers, websocket support

## Deployment Instructions

### Prerequisites
- Kubernetes cluster running
- nginx-ingress-controller installed
- cert-manager installed and configured with `hbg-ca-issuer`
- DNS record for `k8s.hbg.lol` pointing to your ingress controller

### Deploy All Components
Apply all manifests in order:

```bash
# Create namespace first
kubectl apply -f namespace.yaml

# Deploy RBAC resources
kubectl apply -f rbac.yaml

# Deploy dashboard and metrics scraper
kubectl apply -f deployment.yaml

# Create services
kubectl apply -f service.yaml

# Configure ingress
kubectl apply -f ingress.yaml
```

Or deploy everything at once:

```bash
kubectl apply -f /home/user/hbg/k8s/dashboard/
```

### Verify Deployment

```bash
# Check all resources in the namespace
kubectl get all -n kubernetes-dashboard

# Check ingress
kubectl get ingress -n kubernetes-dashboard

# Check certificate generation
kubectl get certificate -n kubernetes-dashboard

# View dashboard logs
kubectl logs -n kubernetes-dashboard deployment/kubernetes-dashboard

# View metrics scraper logs
kubectl logs -n kubernetes-dashboard deployment/dashboard-metrics-scraper
```

## Accessing the Dashboard

### Via Ingress (Production)
Once deployed and DNS is configured, access the dashboard at:
- **URL**: https://k8s.hbg.lol
- **Protocol**: HTTPS (TLS certificate automatically generated)

### Via Port Forward (Development/Testing)
If ingress is not available:

```bash
kubectl port-forward -n kubernetes-dashboard service/kubernetes-dashboard 8443:443
```

Then access at: https://localhost:8443

## Authentication

The dashboard supports multiple authentication methods:

### 1. Token-based Authentication (Recommended)

Get the admin service account token:

```bash
# For Kubernetes 1.24+
kubectl -n kubernetes-dashboard create token kubernetes-dashboard-admin

# For older versions
kubectl -n kubernetes-dashboard get secret $(kubectl -n kubernetes-dashboard get sa kubernetes-dashboard-admin -o jsonpath="{.secrets[0].name}") -o jsonpath="{.data.token}" | base64 --decode
```

Copy the token and paste it into the dashboard login page.

### 2. Create a Long-lived Token Secret (Optional)

For Kubernetes 1.24+, create a long-lived token:

```bash
cat <<EOF | kubectl apply -f -
apiVersion: v1
kind: Secret
metadata:
  name: kubernetes-dashboard-admin-token
  namespace: kubernetes-dashboard
  annotations:
    kubernetes.io/service-account.name: kubernetes-dashboard-admin
type: kubernetes.io/service-account-token
EOF
```

Then retrieve it:

```bash
kubectl get secret kubernetes-dashboard-admin-token -n kubernetes-dashboard -o jsonpath="{.data.token}" | base64 --decode
```

### 3. Skip Authentication (Development Only)
The dashboard is configured with `--enable-skip-login` flag, allowing you to skip authentication.

**WARNING**: Only use this in development/testing environments!

## Security Considerations

### Production Recommendations
1. **Disable skip login**: Remove `--enable-skip-login` from deployment.yaml
2. **Use strong RBAC**: The admin service account has full cluster access - create restricted accounts for regular users
3. **Network policies**: Consider implementing network policies to restrict dashboard access
4. **Audit logging**: Enable audit logging for dashboard access
5. **Regular updates**: Keep dashboard images updated for security patches

### Current Security Features
- TLS encryption via cert-manager
- Security headers (X-Frame-Options, CSP, etc.)
- Rate limiting (50 requests/minute)
- Websocket support for real-time updates
- Read-only root filesystem in containers
- Non-root user execution (UID 1001, GID 2001)

## Troubleshooting

### Dashboard pod not starting
```bash
kubectl describe pod -n kubernetes-dashboard -l app=kubernetes-dashboard
```

### Certificate issues
```bash
kubectl describe certificate kubernetes-dashboard-tls-secret -n kubernetes-dashboard
kubectl logs -n cert-manager deployment/cert-manager
```

### Ingress not working
```bash
kubectl describe ingress kubernetes-dashboard-ingress -n kubernetes-dashboard
kubectl logs -n ingress-nginx deployment/ingress-nginx-controller
```

### Can't access metrics
Check metrics-scraper logs:
```bash
kubectl logs -n kubernetes-dashboard deployment/dashboard-metrics-scraper
```

## Resource Requirements

### Dashboard
- CPU: 100m (request), 500m (limit)
- Memory: 200Mi (request), 500Mi (limit)

### Metrics Scraper
- CPU: 50m (request), 200m (limit)
- Memory: 100Mi (request), 200Mi (limit)

## Cleanup

To remove the dashboard:

```bash
kubectl delete -f /home/user/hbg/k8s/dashboard/
```

Or remove the namespace (deletes everything):

```bash
kubectl delete namespace kubernetes-dashboard
```

## Additional Resources

- [Official Kubernetes Dashboard Documentation](https://kubernetes.io/docs/tasks/access-application-cluster/web-ui-dashboard/)
- [Dashboard GitHub Repository](https://github.com/kubernetes/dashboard)
- [Dashboard Access Control](https://github.com/kubernetes/dashboard/blob/master/docs/user/access-control/README.md)
