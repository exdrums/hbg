# Identity Admin SPA - Deployment Guide

Complete guide for deploying the Identity Admin SPA to Kubernetes.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Local Development](#local-development)
- [Docker Deployment](#docker-deployment)
- [Kubernetes Deployment](#kubernetes-deployment)
- [Production Deployment](#production-deployment)
- [Monitoring & Health Checks](#monitoring--health-checks)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

- **Docker** - 20.10+
- **Kubernetes** - 1.28+
- **kubectl** - 1.28+
- **.NET SDK** - 9.0+
- **Node.js** - 18+
- **npm** - 9+

### Optional Tools

- **Helm** - For templated deployments
- **k9s** - Kubernetes CLI manager
- **kubectx** - Switch between clusters
- **Lens** - Kubernetes IDE

## Local Development

### Running from Source

**Start ASP.NET Core backend:**
```bash
cd src/Services/API/Identity/API.Identity.Admin.Spa
dotnet run
```

**Start Angular development server (separate terminal):**
```bash
cd Client
npm install
npm start
```

**Access the application:**
- Backend: http://localhost:5000
- Angular Dev Server: http://localhost:4201
- Health Checks UI: http://localhost:5000/hc

### Environment Variables

Create `appsettings.Development.json`:

```json
{
  "HBGIDENTITY": "https://sts.hbg.local",
  "HBGIDENTITYADMINSPA": "https://adminspa.hbg.local",
  "HBGIDENTITYADMINSPADEV": "http://localhost:4201",
  "HBGIDENTITYADMINAPI": "https://adminapi.hbg.local",
  "ASPNETCORE_URLS": "http://localhost:5000"
}
```

## Docker Deployment

### Build Docker Image

**From repository root:**
```bash
docker build \
  -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
  -t exdrums/hbg-admin-spa:1.0.0 \
  -t exdrums/hbg-admin-spa:latest \
  .
```

**Build time:** ~5-7 minutes (first build)

### Test Docker Image Locally

```bash
docker run -d \
  --name hbg-admin-spa \
  -p 5796:80 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e HBGIDENTITY=https://sts.hbg.local \
  -e HBGIDENTITYADMINSPA=https://adminspa.hbg.local \
  -e HBGIDENTITYADMINAPI=https://adminapi.hbg.local \
  exdrums/hbg-admin-spa:latest
```

**Verify:**
```bash
# Wait for startup
sleep 15

# Check health
curl http://localhost:5796/health
curl http://localhost:5796/configuration

# View logs
docker logs hbg-admin-spa

# Access in browser
open http://localhost:5796
```

**Stop and remove:**
```bash
docker stop hbg-admin-spa
docker rm hbg-admin-spa
```

### Push to Registry

**Docker Hub:**
```bash
docker login
docker push exdrums/hbg-admin-spa:1.0.0
docker push exdrums/hbg-admin-spa:latest
```

**Private Registry:**
```bash
docker tag exdrums/hbg-admin-spa:latest myregistry.com/hbg-admin-spa:latest
docker push myregistry.com/hbg-admin-spa:latest
```

## Kubernetes Deployment

### Step 1: Prepare Cluster

**Verify cluster access:**
```bash
kubectl cluster-info
kubectl get nodes
```

**Create namespace (optional):**
```bash
kubectl create namespace hbg-system
kubectl config set-context --current --namespace=hbg-system
```

### Step 2: Apply Configuration

**Add domain to /etc/hosts:**
```bash
echo "127.0.0.1 adminspa.hbg.local" | sudo tee -a /etc/hosts
```

**Apply ConfigMap:**
```bash
kubectl apply -f k8s/hbg-configmap.yaml
```

**Verify ConfigMap:**
```bash
kubectl get configmap hbg-configmap
kubectl describe configmap hbg-configmap
```

### Step 3: Deploy Application

**Apply deployment and service:**
```bash
kubectl apply -f k8s/hbg-admin-spa.yaml
```

**Verify deployment:**
```bash
# Check deployment status
kubectl get deployment hbg-admin-spa

# Check pods
kubectl get pods -l app=hbg-admin-spa

# Check service
kubectl get service hbg-admin-spa-service
```

**Wait for pods to be ready:**
```bash
kubectl wait --for=condition=ready pod -l app=hbg-admin-spa --timeout=120s
```

### Step 4: Configure Ingress

**Apply ingress:**
```bash
kubectl apply -f k8s/hbg-ingress.yaml
```

**Verify ingress:**
```bash
kubectl get ingress hbg-ingress
kubectl describe ingress hbg-ingress
```

**Setup port forwarding (for local testing):**
```bash
kubectl port-forward --namespace=ingress-nginx service/ingress-nginx-controller 80:80 443:443
```

### Step 5: Verify Deployment

**Check all resources:**
```bash
kubectl get all -l app=hbg-admin-spa
```

**View logs:**
```bash
kubectl logs -l app=hbg-admin-spa -f
```

**Test health endpoint:**
```bash
kubectl port-forward service/hbg-admin-spa-service 8080:80

# In another terminal:
curl http://localhost:8080/health
curl http://localhost:8080/hc
curl http://localhost:8080/configuration
```

**Access via browser:**
```
https://adminspa.hbg.local/
https://adminspa.hbg.local/hc  (Health Checks UI)
```

## Production Deployment

### Resource Limits

**Recommended limits (adjust based on load):**

```yaml
resources:
  requests:
    memory: "256Mi"
    cpu: "250m"
  limits:
    memory: "512Mi"
    cpu: "1000m"
```

### Scaling

**Horizontal Pod Autoscaler:**

```bash
kubectl autoscale deployment hbg-admin-spa \
  --cpu-percent=70 \
  --min=2 \
  --max=10
```

**Manual scaling:**
```bash
kubectl scale deployment hbg-admin-spa --replicas=3
```

### High Availability

**Update deployment for HA:**

```yaml
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0
  template:
    spec:
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: app
                  operator: In
                  values:
                  - hbg-admin-spa
              topologyKey: kubernetes.io/hostname
```

### TLS/SSL Certificates

**Using cert-manager:**

```yaml
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: adminspa-tls
spec:
  secretName: hbg-tls-secret
  issuerRef:
    name: hbg-ca-issuer
    kind: ClusterIssuer
  dnsNames:
  - adminspa.hbg.local
```

### Environment-Specific Configurations

**Staging:**
```bash
kubectl apply -f k8s/overlays/staging/
```

**Production:**
```bash
kubectl apply -f k8s/overlays/production/
```

## Monitoring & Health Checks

### Health Check Endpoints

- **Liveness Probe:** `GET /health` - Returns 200 if application is alive
- **Readiness Probe:** `GET /health` - Returns 200 if ready to serve traffic
- **Health UI:** `GET /hc` - Interactive health checks dashboard

### Kubernetes Health Probes

**Configured in deployment:**

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 80
  initialDelaySeconds: 40
  periodSeconds: 30
  timeoutSeconds: 3
  failureThreshold: 3

readinessProbe:
  httpGet:
    path: /health
    port: 80
  initialDelaySeconds: 20
  periodSeconds: 10
  timeoutSeconds: 3
  failureThreshold: 3
```

### View Health Status

**From Health Checks UI:**
```
https://adminspa.hbg.local/hc
```

**Check status:**
- Self (Admin SPA)
- API.Identity (STS)
- API.Identity.Admin.Api (Admin API)

### Prometheus Metrics

**Expose metrics endpoint (future enhancement):**
```csharp
// Add to Startup.cs
app.UseHealthChecksPrometheusExporter("/metrics");
```

### Log Aggregation

**View logs in Kubernetes:**
```bash
kubectl logs -l app=hbg-admin-spa -f --tail=100
```

**Using Stern (multi-pod logs):**
```bash
stern hbg-admin-spa
```

## Troubleshooting

### Common Issues

#### 1. Pod CrashLoopBackOff

```bash
# Check pod status
kubectl get pods -l app=hbg-admin-spa

# View pod events
kubectl describe pod <pod-name>

# Check logs
kubectl logs <pod-name>
kubectl logs <pod-name> --previous
```

**Common causes:**
- Missing environment variables
- Database connection issues
- Port conflicts
- Insufficient resources

#### 2. ImagePullBackOff

```bash
# Check image pull status
kubectl describe pod <pod-name>

# Verify image exists
docker images | grep hbg-admin-spa
```

**Solutions:**
- Set `imagePullPolicy: Never` for local images
- Push image to registry
- Create image pull secrets for private registries

#### 3. Health Check Failures

```bash
# Execute into pod
kubectl exec -it <pod-name> -- /bin/bash

# Test health endpoint internally
curl http://localhost:80/health

# Check environment variables
env | grep HBG
```

#### 4. Ingress Not Working

```bash
# Check ingress controller
kubectl get pods -n ingress-nginx

# Check ingress configuration
kubectl describe ingress hbg-ingress

# Test service directly
kubectl port-forward service/hbg-admin-spa-service 8080:80
curl http://localhost:8080/health
```

#### 5. Configuration Issues

```bash
# Verify ConfigMap
kubectl get configmap hbg-configmap -o yaml

# Check pod environment
kubectl exec -it <pod-name> -- env | grep HBG

# Restart pods to pick up new config
kubectl rollout restart deployment hbg-admin-spa
```

### Debug Commands

**Interactive shell:**
```bash
kubectl exec -it <pod-name> -- /bin/bash
```

**View all events:**
```bash
kubectl get events --sort-by='.lastTimestamp'
```

**Resource usage:**
```bash
kubectl top pod -l app=hbg-admin-spa
kubectl top node
```

**Network debugging:**
```bash
kubectl run -it --rm debug --image=nicolaka/netshoot --restart=Never -- /bin/bash
# Inside the pod:
curl http://hbg-admin-spa-service/health
nslookup hbg-admin-spa-service
```

## Cleanup

### Remove Deployment

```bash
kubectl delete -f k8s/hbg-admin-spa.yaml
```

### Remove All Resources

```bash
kubectl delete deployment hbg-admin-spa
kubectl delete service hbg-admin-spa-service
kubectl delete ingress hbg-ingress
kubectl delete configmap hbg-configmap
```

### Complete Cleanup

```bash
# Delete namespace (if using separate namespace)
kubectl delete namespace hbg-system

# Remove local Docker images
docker rmi exdrums/hbg-admin-spa:latest
```

## CI/CD Integration

### GitLab CI Example

```yaml
deploy:
  stage: deploy
  script:
    - kubectl config use-context $KUBE_CONTEXT
    - kubectl apply -f k8s/hbg-configmap.yaml
    - kubectl apply -f k8s/hbg-admin-spa.yaml
    - kubectl rollout status deployment/hbg-admin-spa
  only:
    - main
```

### GitHub Actions Example

```yaml
- name: Deploy to Kubernetes
  run: |
    kubectl apply -f k8s/hbg-admin-spa.yaml
    kubectl rollout status deployment/hbg-admin-spa
```

## Best Practices

1. **Use specific image tags** - Avoid `:latest` in production
2. **Set resource limits** - Prevent resource exhaustion
3. **Configure health checks** - Enable automatic recovery
4. **Use ConfigMaps/Secrets** - Externalize configuration
5. **Enable logging** - Centralize logs for troubleshooting
6. **Monitor metrics** - Track performance and errors
7. **Test deployments** - Validate in staging before production
8. **Document changes** - Maintain deployment history
9. **Backup configurations** - Version control all manifests
10. **Secure secrets** - Use Kubernetes secrets or vault

## Support

For deployment issues:
- Check logs: `kubectl logs -l app=hbg-admin-spa`
- Review events: `kubectl get events`
- Verify configuration: `kubectl describe deployment hbg-admin-spa`
- Test connectivity: Port forward and curl endpoints
