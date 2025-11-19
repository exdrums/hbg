# Uptime Kuma Integration for HBG Kubernetes Services

This directory contains all the necessary Kubernetes configurations and setup scripts to deploy **Uptime Kuma**, a self-hosted monitoring solution, to track the health and uptime of all HBG microservices.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Detailed Setup](#detailed-setup)
- [Configuration Files](#configuration-files)
- [Monitored Services](#monitored-services)
- [Health Check Endpoints](#health-check-endpoints)
- [Accessing Uptime Kuma](#accessing-uptime-kuma)
- [Configuring Monitors](#configuring-monitors)
- [Notifications](#notifications)
- [Troubleshooting](#troubleshooting)
- [Maintenance](#maintenance)
- [Security Considerations](#security-considerations)
- [Backup and Restore](#backup-and-restore)

## Overview

Uptime Kuma is a fancy self-hosted monitoring tool that provides:

- **Real-time monitoring** of HTTP(S), TCP, Ping, DNS, and more
- **Beautiful dashboard** with status pages
- **Multi-protocol support** for comprehensive monitoring
- **Notification system** supporting 90+ services (Slack, Discord, Email, etc.)
- **SSL certificate monitoring** with expiration alerts
- **Status pages** for public or internal use
- **Docker and Kubernetes ready**

## Features

### Monitoring Capabilities

- ✅ HTTP/HTTPS endpoint monitoring
- ✅ TCP port monitoring
- ✅ Ping monitoring
- ✅ DNS monitoring
- ✅ SSL certificate expiration tracking
- ✅ Keyword matching in HTTP responses
- ✅ Custom HTTP headers and authentication
- ✅ WebSocket monitoring
- ✅ Database connection monitoring

### Integration Features

- ✅ Multiple notification channels
- ✅ Customizable status pages
- ✅ Multi-user support with RBAC
- ✅ API for automation
- ✅ Prometheus exporter
- ✅ Mobile-friendly UI
- ✅ Dark mode support

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Kubernetes Cluster                        │
│                                                              │
│  ┌────────────────┐         ┌─────────────────────────┐    │
│  │   Monitoring   │         │   Default Namespace     │    │
│  │   Namespace    │         │                         │    │
│  │                │         │  ┌──────────────────┐   │    │
│  │  ┌──────────┐  │         │  │  API.Identity    │   │    │
│  │  │  Uptime  │  │◄────────┼──│  (STS)           │   │    │
│  │  │  Kuma    │  │         │  └──────────────────┘   │    │
│  │  │          │  │         │                         │    │
│  │  └────┬─────┘  │         │  ┌──────────────────┐   │    │
│  │       │        │◄────────┼──│  Web.SPA         │   │    │
│  │       │        │         │  └──────────────────┘   │    │
│  │  ┌────▼─────┐  │         │                         │    │
│  │  │   PVC    │  │         │  ┌──────────────────┐   │    │
│  │  │  2GB     │  │◄────────┼──│  API.Files       │   │    │
│  │  └──────────┘  │         │  └──────────────────┘   │    │
│  │                │         │                         │    │
│  └────────────────┘         │  ┌──────────────────┐   │    │
│                             │  │  API.Projects    │   │    │
│  ┌────────────────┐         │  └──────────────────┘   │    │
│  │     Ingress    │◄────────┤                         │    │
│  │  monitoring.   │         │  ┌──────────────────┐   │    │
│  │  hbg.lol     │         │  │  API.Contacts    │   │    │
│  └────────────────┘         │  └──────────────────┘   │    │
│                             │                         │    │
│                             │  ┌──────────────────┐   │    │
│                             │  │  API.Emailer     │   │    │
│                             │  └──────────────────┘   │    │
│                             │                         │    │
│                             │  ┌──────────────────┐   │    │
│                             │  │  PostgreSQL DB   │   │    │
│                             │  └──────────────────┘   │    │
│                             └─────────────────────────┘    │
│                                                              │
│  ┌────────────────┐                                         │
│  │  N8N Namespace │                                         │
│  │  ┌──────────┐  │                                         │
│  │  │   N8N    │  │                                         │
│  │  │ Workflow │◄─┼─────────────────────────────────────────┤
│  │  └──────────┘  │                                         │
│  └────────────────┘                                         │
└─────────────────────────────────────────────────────────────┘
```

## Prerequisites

Before deploying Uptime Kuma, ensure you have:

1. **Kubernetes Cluster** (v1.19+)
   - A running Kubernetes cluster with kubectl access
   - Sufficient resources (minimum 500MB RAM, 250m CPU)

2. **Storage Provisioner**
   - Dynamic volume provisioning or pre-created PersistentVolume
   - At least 2GB of available storage

3. **Ingress Controller**
   - NGINX Ingress Controller (configured in existing setup)
   - Ingress class: `nginx`

4. **TLS/SSL Certificates**
   - cert-manager installed and configured
   - CA Issuer: `hbg-ca-issuer` (already configured)

5. **DNS Configuration**
   - Ability to add `monitoring.hbg.lol` to your DNS or `/etc/hosts`

6. **Tools**
   - `kubectl` CLI tool
   - `bash` (for running setup scripts)

## Quick Start

The fastest way to deploy Uptime Kuma:

```bash
cd /home/user/hbg/k8s/uptime-kuma

# Deploy all components
./setup.sh deploy

# Wait for deployment to be ready (automatically done in deploy)
./setup.sh wait

# Check status
./setup.sh status

# View logs
./setup.sh logs

# Access information
./setup.sh access
```

## Detailed Setup

### Step 1: Review Configuration Files

Before deploying, review the configuration files in this directory:

```bash
ls -la /home/user/hbg/k8s/uptime-kuma/
```

Files you should see:
- `namespace.yaml` - Creates the monitoring namespace
- `storage.yaml` - PersistentVolumeClaim for data persistence
- `deployment.yaml` - Uptime Kuma deployment configuration
- `service.yaml` - ClusterIP service
- `ingress.yaml` - Ingress with TLS
- `monitors-config.json` - Reference configuration for monitors
- `setup.sh` - Setup automation script
- `README.md` - This file

### Step 2: Deploy Namespace

```bash
kubectl apply -f namespace.yaml
```

Verify:
```bash
kubectl get namespace monitoring
```

### Step 3: Create Persistent Storage

```bash
kubectl apply -f storage.yaml
```

Verify:
```bash
kubectl get pvc -n monitoring
```

Wait for the PVC to be bound:
```bash
kubectl wait --for=jsonpath='{.status.phase}'=Bound pvc/uptime-kuma-pvc -n monitoring --timeout=60s
```

### Step 4: Deploy Uptime Kuma

```bash
kubectl apply -f deployment.yaml
```

Verify:
```bash
kubectl get deployment -n monitoring
kubectl get pods -n monitoring
```

### Step 5: Create Service

```bash
kubectl apply -f service.yaml
```

Verify:
```bash
kubectl get service -n monitoring
```

### Step 6: Create Ingress

```bash
kubectl apply -f ingress.yaml
```

Verify:
```bash
kubectl get ingress -n monitoring
```

### Step 7: Configure DNS

Add the following entry to your `/etc/hosts` file (or your DNS server):

```bash
# Get your cluster ingress IP
kubectl get ingress -n monitoring uptime-kuma-ingress -o jsonpath='{.status.loadBalancer.ingress[0].ip}'

# Add to /etc/hosts
sudo sh -c 'echo "<CLUSTER_IP> monitoring.hbg.lol" >> /etc/hosts'
```

### Step 8: Access Uptime Kuma

Open your browser and navigate to:
```
https://monitoring.hbg.lol
```

**First-time setup:**
1. Create an admin account (username and password)
2. Complete the initial setup wizard
3. You'll be redirected to the dashboard

## Configuration Files

### namespace.yaml

Creates a dedicated `monitoring` namespace for Uptime Kuma:

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: monitoring
  labels:
    name: monitoring
    purpose: observability
```

### storage.yaml

Defines a PersistentVolumeClaim for data persistence:

- **Size**: 2GB
- **Access Mode**: ReadWriteOnce
- **Storage Class**: Uses default (can be customized)

All Uptime Kuma data (configuration, monitors, historical data) is stored here.

### deployment.yaml

Key configuration:
- **Image**: `louislam/uptime-kuma:1` (latest stable)
- **Replicas**: 1 (must be 1 due to SQLite database)
- **Port**: 3001
- **Resources**:
  - Requests: 256Mi RAM, 250m CPU
  - Limits: 1Gi RAM, 1000m CPU
- **Probes**: Liveness, Readiness, and Startup probes configured

### service.yaml

- **Type**: ClusterIP (internal only)
- **Port**: 3001
- **Session Affinity**: ClientIP (important for WebSocket connections)
- **Session Timeout**: 3 hours

### ingress.yaml

- **Host**: monitoring.hbg.lol
- **TLS**: Enabled with cert-manager
- **Annotations**:
  - WebSocket support
  - Security headers
  - Rate limiting (100 req/s)
  - Timeouts (3600s for long-polling)

## Monitored Services

Uptime Kuma can monitor the following HBG services:

### External Services (via Ingress)

| Service | Health Endpoint | Type | Priority |
|---------|----------------|------|----------|
| API.Identity (STS) | `https://sts.hbg.lol/health` | HTTP | Critical |
| API.Identity (STS) Ready | `https://sts.hbg.lol/health/ready` | HTTP | High |
| Web.SPA | `https://spa.hbg.lol/health` | HTTP | Critical |
| API.Identity.Admin.UI | `https://admin.hbg.lol/health` | HTTP | Medium |
| N8N Workflow | `https://n8n.hbg.lol` | HTTP | Medium |

### Internal Services (via Cluster DNS)

| Service | Health Endpoint | Type | Priority |
|---------|----------------|------|----------|
| API.Files | `http://hbg-files-service.default.svc.cluster.local/health` | HTTP | Medium |
| API.Projects | `http://hbg-projects-service.default.svc.cluster.local/health` | HTTP | Medium |
| API.Contacts | `http://hbg-contacts-service.default.svc.cluster.local/health` | HTTP | Medium |
| API.Emailer | `http://hbg-emailer-service.default.svc.cluster.local/health` | HTTP | Medium |
| PostgreSQL | `hbg-db.default.svc.cluster.local:5432` | TCP Port | Critical |

### Infrastructure

| Service | Endpoint | Type | Priority |
|---------|----------|------|----------|
| Uptime Kuma (Self) | `http://uptime-kuma-service.monitoring.svc.cluster.local:3001` | HTTP | High |

## Health Check Endpoints

All HBG services now implement standardized health check endpoints:

### Standard Endpoints

1. **`/health`** - Complete health status
   - Returns: JSON with all health checks
   - Use for: Comprehensive monitoring

2. **`/health/ready`** - Readiness probe
   - Returns: JSON with dependency checks (DB, external services)
   - Use for: Kubernetes readiness probes and availability monitoring

3. **`/health/live`** - Liveness probe
   - Returns: JSON with no dependencies
   - Use for: Kubernetes liveness probes

### Example Response

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "description": "Database connection is healthy",
      "duration": 12.5,
      "tags": ["ready", "db"]
    },
    {
      "name": "signalr",
      "status": "Healthy",
      "description": "SignalR is ready",
      "duration": 0.2,
      "tags": []
    }
  ],
  "totalDuration": 12.7,
  "timestamp": "2025-11-10T10:30:45.123Z"
}
```

## Accessing Uptime Kuma

### Option 1: Via Ingress (Recommended)

```
https://monitoring.hbg.lol
```

Requirements:
- DNS entry or `/etc/hosts` configuration
- Valid TLS certificate (handled by cert-manager)

### Option 2: Port Forwarding (Development)

For local access without DNS configuration:

```bash
./setup.sh port-forward
# or
kubectl port-forward -n monitoring svc/uptime-kuma-service 3001:3001
```

Then access:
```
http://localhost:3001
```

### Option 3: NodePort (Not Recommended)

You can modify the service to use NodePort if needed, but this is not recommended for production.

## Configuring Monitors

### Manual Configuration (Recommended)

1. Log in to Uptime Kuma web interface
2. Click **"Add New Monitor"**
3. Fill in monitor details:

**Example: API.Identity Monitor**
```
Name: API.Identity (STS)
Monitor Type: HTTP(s)
URL: https://sts.hbg.lol/health
Heartbeat Interval: 60 seconds
Max Retries: 3
Retry Interval: 60 seconds
Heartbeat Timeout: 30 seconds
Request Method: GET
HTTP Status: 200
Tags: api, identity, critical
```

4. Click **"Save"**

### Using the Reference Configuration

The `monitors-config.json` file contains pre-configured monitor definitions. Use this as a reference when manually creating monitors in the UI.

**Note**: Uptime Kuma does not currently support automated configuration import via config files. However, you can use the Uptime Kuma API for programmatic monitor creation.

### API-Based Configuration (Advanced)

Uptime Kuma exposes an API for automation. Example:

```bash
# Get API token from Settings > Security
API_TOKEN="your-api-token-here"

# Create a monitor via API
curl -X POST https://monitoring.hbg.lol/api/monitor \
  -H "Authorization: Bearer $API_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "http",
    "name": "API.Identity",
    "url": "https://sts.hbg.lol/health",
    "interval": 60,
    "maxretries": 3
  }'
```

Refer to the [Uptime Kuma API documentation](https://github.com/louislam/uptime-kuma/wiki/API) for more details.

## Notifications

Uptime Kuma supports 90+ notification services. Common ones for HBG:

### Email Notifications

1. Go to **Settings** > **Notifications**
2. Click **"Setup Notification"**
3. Select **"Email (SMTP)"**
4. Configure:
   - SMTP Host: `smtp.example.com`
   - SMTP Port: `587`
   - Secure: Enable TLS
   - Username: `alerts@example.com`
   - Password: `your-password`
   - From Email: `uptime-kuma@hbg.lol`
   - To Email: `admin@example.com`

### Slack Notifications

1. Create a Slack Incoming Webhook
2. Go to **Settings** > **Notifications**
3. Select **"Slack"**
4. Paste webhook URL
5. Test and save

### Discord Notifications

1. Create a Discord Webhook in your server
2. Go to **Settings** > **Notifications**
3. Select **"Discord"**
4. Paste webhook URL
5. Test and save

### Custom Webhooks

For integration with other systems:

1. Select **"Webhook"**
2. Configure:
   - POST URL: Your webhook endpoint
   - Content Type: JSON
   - Custom Body:
   ```json
   {
     "service": "[monitorName]",
     "status": "[status]",
     "message": "[msg]",
     "timestamp": "[datetime]"
   }
   ```

## Troubleshooting

### Pod Not Starting

**Check pod status:**
```bash
kubectl get pods -n monitoring
kubectl describe pod -n monitoring -l app=uptime-kuma
```

**Check logs:**
```bash
kubectl logs -n monitoring -l app=uptime-kuma
```

**Common issues:**
- PVC not bound: Check storage provisioner
- Image pull errors: Check internet connectivity
- Resource limits: Increase CPU/memory limits

### PVC Not Binding

**Check PVC status:**
```bash
kubectl get pvc -n monitoring
kubectl describe pvc uptime-kuma-pvc -n monitoring
```

**Solutions:**
- Install a storage provisioner (e.g., local-path-provisioner)
- Create a PersistentVolume manually
- Use `hostPath` for testing (not recommended for production)

### Cannot Access via Ingress

**Check ingress:**
```bash
kubectl get ingress -n monitoring
kubectl describe ingress uptime-kuma-ingress -n monitoring
```

**Common issues:**
- DNS not configured: Add to `/etc/hosts`
- Ingress controller not installed
- TLS certificate not issued: Check cert-manager logs

**Check cert-manager:**
```bash
kubectl get certificate -n monitoring
kubectl describe certificate uptime-kuma-tls-secret -n monitoring
```

### WebSocket Connection Issues

Uptime Kuma uses WebSockets for real-time updates. If the UI doesn't update:

**Check ingress annotations:**
```bash
kubectl get ingress uptime-kuma-ingress -n monitoring -o yaml | grep websocket
```

Should include:
```yaml
nginx.ingress.kubernetes.io/websocket-services: "uptime-kuma-service"
```

**Check browser console** for WebSocket errors.

### Database Locked Errors

Uptime Kuma uses SQLite. If you see "database is locked" errors:

**Solution:**
- Ensure only 1 replica is running
- Restart the pod:
```bash
kubectl rollout restart deployment/uptime-kuma -n monitoring
```

### High Memory Usage

If Uptime Kuma is using too much memory:

**Check current usage:**
```bash
kubectl top pod -n monitoring
```

**Solutions:**
- Reduce number of monitors
- Increase check intervals
- Increase memory limits in deployment.yaml
- Reduce retention period for historical data

## Maintenance

### Updating Uptime Kuma

To update to the latest version:

```bash
# Edit deployment to use latest tag
kubectl set image deployment/uptime-kuma uptime-kuma=louislam/uptime-kuma:latest -n monitoring

# Or edit deployment.yaml and apply
kubectl apply -f deployment.yaml

# Check rollout status
kubectl rollout status deployment/uptime-kuma -n monitoring
```

### Restarting Uptime Kuma

```bash
./setup.sh restart
# or
kubectl rollout restart deployment/uptime-kuma -n monitoring
```

### Viewing Logs

```bash
./setup.sh logs
# or
kubectl logs -n monitoring -l app=uptime-kuma --tail=100 -f
```

### Scaling Considerations

**Important**: Uptime Kuma uses SQLite and **cannot be scaled horizontally**. Always keep replicas at 1.

For high availability, consider:
- Database backups (see Backup section)
- Regular snapshots of PVC
- Monitoring the monitor (meta-monitoring)

## Security Considerations

### Authentication

- **Always** set a strong admin password on first setup
- Enable two-factor authentication (2FA) if available
- Use unique passwords for each user

### Network Security

- Ingress is configured with TLS/SSL
- Rate limiting is enabled (100 req/s)
- Security headers are configured
- WebSocket connections are secured

### Secrets Management

- API tokens should be stored in Kubernetes Secrets
- Never commit tokens to git
- Rotate tokens regularly

### Access Control

- Limit access to monitoring namespace
- Use RBAC to control who can access Uptime Kuma
- Consider using OAuth2 proxy for SSO

### SSL/TLS Certificates

- Certificates are managed by cert-manager
- Ensure CA certificates are trusted by monitored services
- Monitor certificate expiration dates

## Backup and Restore

### Backup Uptime Kuma Data

Uptime Kuma stores all data in `/app/data` (mounted from PVC).

**Option 1: Manual Backup**

```bash
# Create a backup pod
kubectl run backup-pod --image=busybox -n monitoring -- sleep 3600

# Copy data from PVC
kubectl cp monitoring/backup-pod:/app/data ./uptime-kuma-backup

# Delete backup pod
kubectl delete pod backup-pod -n monitoring
```

**Option 2: PVC Snapshot**

If your storage class supports snapshots:

```bash
# Create a VolumeSnapshot
kubectl create -f - <<EOF
apiVersion: snapshot.storage.k8s.io/v1
kind: VolumeSnapshot
metadata:
  name: uptime-kuma-snapshot
  namespace: monitoring
spec:
  volumeSnapshotClassName: <your-snapshot-class>
  source:
    persistentVolumeClaimName: uptime-kuma-pvc
EOF
```

**Option 3: Automated Backups with Velero**

For production, consider using Velero for cluster-level backups.

### Restore Uptime Kuma Data

**From Manual Backup:**

```bash
# Scale down deployment
kubectl scale deployment uptime-kuma -n monitoring --replicas=0

# Create restore pod
kubectl run restore-pod --image=busybox -n monitoring -- sleep 3600

# Copy backup to pod
kubectl cp ./uptime-kuma-backup monitoring/restore-pod:/app/data

# Scale up deployment
kubectl scale deployment uptime-kuma -n monitoring --replicas=1

# Delete restore pod
kubectl delete pod restore-pod -n monitoring
```

**From VolumeSnapshot:**

Create a new PVC from the snapshot and update the deployment to use it.

## Advanced Configuration

### Custom Resource Limits

Edit `deployment.yaml` to adjust resources:

```yaml
resources:
  requests:
    memory: "512Mi"  # Increased
    cpu: "500m"      # Increased
  limits:
    memory: "2Gi"    # Increased
    cpu: "2000m"     # Increased
```

Apply:
```bash
kubectl apply -f deployment.yaml
```

### Using External Database (PostgreSQL)

Uptime Kuma 2.x supports PostgreSQL. To use the existing HBG PostgreSQL:

1. Edit `deployment.yaml` and add environment variables:
```yaml
env:
- name: DATABASE
  value: "postgres"
- name: DB_HOST
  value: "hbg-db.default.svc.cluster.local"
- name: DB_PORT
  value: "5432"
- name: DB_NAME
  value: "uptimekuma"
- name: DB_USER
  valueFrom:
    secretKeyRef:
      name: hbg-secret
      key: hbgdb_username
- name: DB_PASSWORD
  valueFrom:
    secretKeyRef:
      name: hbg-secret
      key: hbgdb_password
```

2. Create the database:
```bash
kubectl exec -it hbg-db-<pod-id> -- psql -U postgres -c "CREATE DATABASE uptimekuma;"
```

3. Redeploy Uptime Kuma

### Prometheus Metrics Export

Uptime Kuma can export metrics for Prometheus:

1. Enable metrics in Uptime Kuma settings
2. Create a ServiceMonitor:

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: uptime-kuma
  namespace: monitoring
spec:
  selector:
    matchLabels:
      app: uptime-kuma
  endpoints:
  - port: http
    path: /metrics
    interval: 30s
```

## Support and Resources

### Official Resources

- [Uptime Kuma GitHub](https://github.com/louislam/uptime-kuma)
- [Uptime Kuma Wiki](https://github.com/louislam/uptime-kuma/wiki)
- [Uptime Kuma API Docs](https://github.com/louislam/uptime-kuma/wiki/API)

### HBG-Specific Resources

- Main README: `/home/user/hbg/README.md`
- Kubernetes configs: `/home/user/hbg/k8s/`
- Service health checks: Implemented in each service's `Pipeline.cs`

### Getting Help

For issues with:
- **Uptime Kuma**: Check GitHub issues
- **HBG integration**: Review this README and service logs
- **Kubernetes**: Check pod logs and describe resources

## Cleanup

To completely remove Uptime Kuma:

```bash
./setup.sh delete
```

Or manually:

```bash
kubectl delete -f ingress.yaml
kubectl delete -f service.yaml
kubectl delete -f deployment.yaml
kubectl delete -f storage.yaml
kubectl delete namespace monitoring
```

**Warning**: This will delete all Uptime Kuma data permanently!

## Summary

You now have a fully functional Uptime Kuma deployment monitoring all HBG services. Key points:

✅ **Deployed** Uptime Kuma to `monitoring` namespace
✅ **Configured** health checks for all services
✅ **Exposed** via Ingress at `monitoring.hbg.lol`
✅ **Persistent** storage for data retention
✅ **Ready** to configure monitors and notifications

Next steps:
1. Access the web UI and complete initial setup
2. Create monitors for all services using `monitors-config.json` as reference
3. Configure notification channels
4. Create status pages if needed
5. Set up regular backups

Happy Monitoring! 🎉
