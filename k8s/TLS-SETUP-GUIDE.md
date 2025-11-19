# Complete TLS Setup Guide for Azure Kubernetes Service

This guide provides step-by-step instructions to set up production-grade TLS/HTTPS for your Kubernetes cluster in Azure using Let's Encrypt and cert-manager.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Overview](#overview)
3. [Step 1: Verify Prerequisites](#step-1-verify-prerequisites)
4. [Step 2: Install cert-manager](#step-2-install-cert-manager)
5. [Step 3: Configure Let's Encrypt Issuers](#step-3-configure-lets-encrypt-issuers)
6. [Step 4: Configure DNS Records](#step-4-configure-dns-records)
7. [Step 5: Update Ingress Configuration](#step-5-update-ingress-configuration)
8. [Step 6: Deploy and Verify](#step-6-deploy-and-verify)
9. [Step 7: Advanced Configuration](#step-7-advanced-configuration)
10. [Troubleshooting](#troubleshooting)
11. [Best Practices](#best-practices)

---

## Prerequisites

Before starting, ensure you have:

- ✅ Azure Kubernetes Service (AKS) cluster running
- ✅ NGINX Ingress Controller installed
- ✅ Domain name registered (e.g., `hbg.lol`)
- ✅ DNS access to configure A records
- ✅ `kubectl` configured and connected to your cluster
- ✅ Azure CLI installed (optional, for Azure DNS)

---

## Overview

This setup uses:
- **cert-manager**: Automatically manages TLS certificates
- **Let's Encrypt**: Free, trusted SSL certificates
- **HTTP-01 Challenge**: Domain validation via HTTP requests
- **Automatic Renewal**: Certificates auto-renew before expiration

**Architecture Flow:**
```
Domain Request → NGINX Ingress → cert-manager → Let's Encrypt → Certificate Issued → TLS Enabled
```

---

## Step 1: Verify Prerequisites

### 1.1 Check NGINX Ingress Controller

```bash
# Verify ingress controller is running
kubectl get pods -n ingress-nginx

# Get the external IP address
kubectl get service ingress-nginx-controller -n ingress-nginx

# Expected output:
# NAME                       TYPE           CLUSTER-IP    EXTERNAL-IP     PORT(S)
# ingress-nginx-controller   LoadBalancer   10.x.x.x     20.x.x.x        80:xxxxx/TCP,443:xxxxx/TCP
```

**Save the EXTERNAL-IP** - you'll need it for DNS configuration.

### 1.2 Verify Cluster Access

```bash
# Test kubectl connection
kubectl cluster-info
kubectl get nodes
```

---

## Step 2: Install cert-manager

### 2.1 Install cert-manager CRDs and Controller

```bash
# Install cert-manager
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.3/cert-manager.yaml

# Wait for cert-manager to be ready (takes 1-2 minutes)
kubectl wait --for=condition=ready pod \
  -l app.kubernetes.io/instance=cert-manager \
  -n cert-manager \
  --timeout=300s

# Verify installation
kubectl get pods -n cert-manager
```

**Expected output:**
```
NAME                                       READY   STATUS    RESTARTS   AGE
cert-manager-xxxxx                        1/1     Running   0          2m
cert-manager-cainjector-xxxxx             1/1     Running   0          2m
cert-manager-webhook-xxxxx                1/1     Running   0          2m
```

### 2.2 Verify cert-manager Installation

```bash
# Check ClusterIssuer CRD is available
kubectl get crd clusterissuers.cert-manager.io

# Check Certificate CRD is available
kubectl get crd certificates.cert-manager.io
```

---

## Step 3: Configure Let's Encrypt Issuers

### 3.1 Create Let's Encrypt Staging Issuer (for Testing)

**Why staging first?** Let's Encrypt has rate limits. Use staging to test without hitting limits.

Create `cert-manager/letsencrypt-staging-issuer.yaml`:

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-staging
spec:
  acme:
    # Let's Encrypt staging server (no rate limits)
    server: https://acme-staging-v02.api.letsencrypt.org/directory
    # Your email for Let's Encrypt notifications
    email: YOUR_EMAIL@example.com  # REPLACE THIS
    # Secret to store ACME account private key
    privateKeySecretRef:
      name: letsencrypt-staging-key
    # HTTP01 challenge solver
    solvers:
    - http01:
        ingress:
          class: nginx
```

### 3.2 Create Let's Encrypt Production Issuer

Update `cert-manager/letsencrypt-issuer.yaml`:

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    # Let's Encrypt production server
    server: https://acme-v02.api.letsencrypt.org/directory
    # Your email for Let's Encrypt notifications
    email: YOUR_EMAIL@example.com  # REPLACE THIS
    # Secret to store ACME account private key
    privateKeySecretRef:
      name: letsencrypt-prod-key
    # HTTP01 challenge solver
    solvers:
    - http01:
        ingress:
          class: nginx
```

**Important:** Replace `YOUR_EMAIL@example.com` with your actual email address.

### 3.3 Apply the Issuers

```bash
# Apply staging issuer (for testing)
kubectl apply -f cert-manager/letsencrypt-staging-issuer.yaml

# Apply production issuer
kubectl apply -f cert-manager/letsencrypt-issuer.yaml

# Verify issuers are ready
kubectl get clusterissuer

# Check issuer status
kubectl describe clusterissuer letsencrypt-staging
kubectl describe clusterissuer letsencrypt-prod
```

**Expected output:**
```
NAME                  READY   AGE
letsencrypt-prod      True    30s
letsencrypt-staging   True    30s
```

---

## Step 4: Configure DNS Records

### 4.1 Get Ingress External IP

```bash
# Get the external IP
INGRESS_IP=$(kubectl get service ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "Ingress IP: $INGRESS_IP"
```

### 4.2 Configure DNS Records

You need to create A records pointing your domains to the ingress IP.

**For Porkbun (or your DNS provider):**

1. Log in to your DNS provider
2. Navigate to DNS settings for your domain
3. Add the following A records:

| Type | Name | Value | TTL |
|------|------|-------|-----|
| A | `sts` | `<INGRESS_IP>` | 300 |
| A | `admin` | `<INGRESS_IP>` | 300 |
| A | `adminspa` | `<INGRESS_IP>` | 300 |
| A | `adminapi` | `<INGRESS_IP>` | 300 |
| A | `spa` | `<INGRESS_IP>` | 300 |
| A | `constructor` | `<INGRESS_IP>` | 300 |
| A | `filesapi` | `<INGRESS_IP>` | 300 |
| A | `n8n` | `<INGRESS_IP>` | 300 |
| A | `monitoring` | `<INGRESS_IP>` | 300 |

**Or use a wildcard record:**
| Type | Name | Value | TTL |
|------|------|-------|-----|
| A | `*` | `<INGRESS_IP>` | 300 |

### 4.3 Verify DNS Propagation

```bash
# Wait a few minutes for DNS propagation, then verify
dig sts.yourdomain.com +short
nslookup sts.yourdomain.com

# All should return your ingress IP
```

**DNS propagation typically takes 5-60 minutes.**

---

## Step 5: Update Ingress Configuration

### 5.1 Update Main Ingress

Update `k8s/hbg-ingress.yaml` to use Let's Encrypt:

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: hbg-ingress
  annotations:
    # Use Let's Encrypt production issuer
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    
    # Force HTTPS redirect
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/force-ssl-redirect: "true"
    
    # Security headers
    nginx.ingress.kubernetes.io/configuration-snippet: |
      add_header X-Frame-Options "SAMEORIGIN" always;
      add_header X-Content-Type-Options "nosniff" always;
      add_header X-XSS-Protection "1; mode=block" always;
      add_header Referrer-Policy "strict-origin-when-cross-origin" always;
      add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    
    # Request size limit
    nginx.ingress.kubernetes.io/proxy-body-size: "100m"
    
    # Timeout settings
    nginx.ingress.kubernetes.io/proxy-connect-timeout: "600"
    nginx.ingress.kubernetes.io/proxy-send-timeout: "600"
    nginx.ingress.kubernetes.io/proxy-read-timeout: "600"
    
    # Rate limiting
    nginx.ingress.kubernetes.io/rate-limit: "100"
    nginx.ingress.kubernetes.io/rate-limit-window: "1m"
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - sts.yourdomain.com
    - admin.yourdomain.com
    - adminspa.yourdomain.com
    - adminapi.yourdomain.com
    - spa.yourdomain.com
    - constructor.yourdomain.com
    - filesapi.yourdomain.com
    - n8n.yourdomain.com
    secretName: hbg-tls-secret
  rules:
  # ... your existing rules ...
```

**Key changes:**
- Changed `cert-manager.io/cluster-issuer` from `hbg-ca-issuer` to `letsencrypt-prod`
- Added `force-ssl-redirect` for stronger HTTPS enforcement
- Added `Strict-Transport-Security` header
- Updated domain names to match your actual domain

### 5.2 Update Other Ingress Resources

Update any other ingress files (e.g., `uptime-kuma/ingress.yaml`) similarly:

```yaml
annotations:
  cert-manager.io/cluster-issuer: "letsencrypt-prod"
  nginx.ingress.kubernetes.io/ssl-redirect: "true"
```

---

## Step 6: Deploy and Verify

### 6.1 Apply Updated Ingress

```bash
# Apply the updated ingress
kubectl apply -f k8s/hbg-ingress.yaml

# Watch for certificate creation
kubectl get certificates --watch
```

### 6.2 Monitor Certificate Issuance

```bash
# Check certificate status
kubectl get certificates

# Describe certificate for details
kubectl describe certificate hbg-tls-secret -n default

# Check certificate requests
kubectl get certificaterequests

# Check challenges (HTTP-01 validation)
kubectl get challenges
```

**Expected certificate status:**
```
NAME            READY   SECRET              AGE
hbg-cert        True    hbg-tls-secret     2m
```

### 6.3 Verify TLS Secret

```bash
# Check TLS secret was created
kubectl get secret hbg-tls-secret -n default

# View certificate details
kubectl get secret hbg-tls-secret -n default -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -text -noout
```

### 6.4 Test HTTPS Access

```bash
# Test HTTPS endpoints (replace with your domain)
curl -I https://sts.yourdomain.com
curl -I https://admin.yourdomain.com
curl -I https://spa.yourdomain.com

# Check certificate validity
openssl s_client -connect sts.yourdomain.com:443 -servername sts.yourdomain.com < /dev/null
```

**Expected:** All endpoints should return `200 OK` with valid SSL certificates.

### 6.5 Verify Automatic Renewal

Let's Encrypt certificates are valid for 90 days and auto-renew at 60 days.

```bash
# Check certificate expiration
kubectl get certificate hbg-cert -o jsonpath='{.status.notAfter}'

# Monitor renewal (cert-manager handles this automatically)
kubectl get events --field-selector involvedObject.name=hbg-cert --sort-by='.lastTimestamp'
```

---

## Step 7: Advanced Configuration

### 7.1 Wildcard Certificates (DNS-01 Challenge)

For wildcard certificates (`*.yourdomain.com`), use DNS-01 challenge instead of HTTP-01.

**Note:** This requires DNS provider API access. See cert-manager DNS01 documentation for your provider.

### 7.2 Multiple Certificates per Domain

If you need separate certificates:

```yaml
apiVersion: cert-manager.io/v1
kind: Certificate
metadata:
  name: admin-cert
  namespace: default
spec:
  secretName: admin-tls-secret
  issuerRef:
    name: letsencrypt-prod
    kind: ClusterIssuer
  dnsNames:
  - "admin.yourdomain.com"
  - "adminapi.yourdomain.com"
```

### 7.3 Certificate Monitoring

Set up alerts for certificate expiration:

```bash
# Create a monitoring script
cat > check-cert-expiry.sh << 'EOF'
#!/bin/bash
CERT_NAME="hbg-cert"
NAMESPACE="default"
DAYS_WARNING=30

EXPIRY=$(kubectl get certificate $CERT_NAME -n $NAMESPACE -o jsonpath='{.status.notAfter}')
EXPIRY_EPOCH=$(date -j -f "%Y-%m-%dT%H:%M:%SZ" "$EXPIRY" +%s 2>/dev/null || date -d "$EXPIRY" +%s)
NOW_EPOCH=$(date +%s)
DAYS_LEFT=$(( ($EXPIRY_EPOCH - $NOW_EPOCH) / 86400 ))

if [ $DAYS_LEFT -lt $DAYS_WARNING ]; then
  echo "WARNING: Certificate expires in $DAYS_LEFT days!"
  exit 1
fi
echo "Certificate valid for $DAYS_LEFT days"
EOF

chmod +x check-cert-expiry.sh
```

### 7.4 Azure Key Vault Integration (Optional)

For enterprise environments, integrate with Azure Key Vault:

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: azure-keyvault-issuer
spec:
  vault:
    server: https://your-keyvault.vault.azure.net
    path: certificates
    auth:
      azureServicePrincipal:
        clientId: <client-id>
        clientSecret:
          name: azure-keyvault-secret
          key: client-secret
        tenantId: <tenant-id>
```

---

## Troubleshooting

### Certificate Not Issued

**Symptoms:** Certificate status shows `False` or `Pending`

```bash
# Check certificate details
kubectl describe certificate hbg-cert

# Check certificate requests
kubectl describe certificaterequest <request-name>

# Check challenges
kubectl describe challenge <challenge-name>

# Check cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager --tail=100
```

**Common issues:**
1. **DNS not configured**: Verify DNS records point to ingress IP
2. **Ingress not accessible**: Check ingress controller is running
3. **Rate limiting**: Use staging issuer for testing
4. **Wrong issuer**: Verify `cert-manager.io/cluster-issuer` annotation

### Certificate Stuck in Pending

```bash
# Delete and recreate certificate
kubectl delete certificate hbg-cert
kubectl apply -f cert-manager/hbg-cert.yaml

# Or let ingress recreate it automatically
kubectl delete ingress hbg-ingress
kubectl apply -f k8s/hbg-ingress.yaml
```

### HTTP-01 Challenge Failing

```bash
# Check ingress is accessible
curl -I http://sts.yourdomain.com/.well-known/acme-challenge/test

# Verify ingress controller logs
kubectl logs -n ingress-nginx -l app.kubernetes.io/component=controller --tail=50

# Check cert-manager can create ingress
kubectl get ingress -l acme.cert-manager.io/http01-solver
```

### Certificate Expired

```bash
# Force renewal
kubectl delete secret hbg-tls-secret
kubectl delete certificate hbg-cert
# cert-manager will recreate automatically
```

### Wrong Certificate Issued

```bash
# Check issuer configuration
kubectl describe clusterissuer letsencrypt-prod

# Verify ingress annotation
kubectl get ingress hbg-ingress -o yaml | grep cert-manager
```

### Rate Limit Exceeded

Let's Encrypt has rate limits:
- **50 certificates per registered domain per week**
- **5 duplicate certificates per week**

**Solution:**
- Use staging issuer for testing
- Wait for rate limit reset
- Request fewer certificates (use wildcard if possible)

---

## Best Practices

### 1. Use Staging First

Always test with staging issuer first:

```yaml
cert-manager.io/cluster-issuer: "letsencrypt-staging"
```

Then switch to production once verified.

### 2. Monitor Certificate Expiration

Set up monitoring to alert before certificates expire (cert-manager auto-renews, but monitoring provides safety).

### 3. Use Appropriate Certificate Scopes

- **Single domain**: One certificate per domain
- **Multiple subdomains**: One certificate with multiple `dnsNames`
- **Wildcard**: Use DNS-01 challenge for `*.yourdomain.com`

### 4. Security Headers

Always include security headers in ingress:

```yaml
nginx.ingress.kubernetes.io/configuration-snippet: |
  add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
```

### 5. Backup Certificates

While cert-manager auto-renews, backup certificates for disaster recovery:

```bash
# Export certificate
kubectl get secret hbg-tls-secret -o yaml > hbg-tls-secret-backup.yaml
```

### 6. Use Separate Namespaces

Organize certificates by namespace:

```yaml
apiVersion: cert-manager.io/v1
kind: Issuer  # Not ClusterIssuer
metadata:
  name: letsencrypt-prod
  namespace: production
```

### 7. Document Your Setup

Keep track of:
- Domain names and their purposes
- Certificate expiration dates
- DNS provider and access
- Issuer configurations

---

## Quick Reference Commands

```bash
# Check certificate status
kubectl get certificates

# View certificate details
kubectl describe certificate <cert-name>

# Check TLS secret
kubectl get secret <secret-name>

# View certificate expiration
kubectl get certificate <cert-name> -o jsonpath='{.status.notAfter}'

# Check cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager

# Force certificate renewal
kubectl delete secret <tls-secret-name>
kubectl delete certificate <cert-name>

# Test HTTPS
curl -I https://yourdomain.com
openssl s_client -connect yourdomain.com:443 -servername yourdomain.com
```

---

## Next Steps

After setting up TLS:

1. ✅ **Monitor certificates**: Set up alerts for expiration
2. ✅ **Test all endpoints**: Verify HTTPS works for all services
3. ✅ **Update application configs**: Ensure apps use HTTPS URLs
4. ✅ **Set up backups**: Backup certificates and keys
5. ✅ **Document DNS**: Keep DNS records documented
6. ✅ **Review security**: Ensure security headers are configured
7. ✅ **Set up monitoring**: Monitor certificate health

---

## Additional Resources

- [cert-manager Documentation](https://cert-manager.io/docs/)
- [Let's Encrypt Documentation](https://letsencrypt.org/docs/)
- [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/)
- [Azure Kubernetes Service](https://docs.microsoft.com/azure/aks/)

---

## Support

If you encounter issues:

1. Check cert-manager logs: `kubectl logs -n cert-manager -l app=cert-manager`
2. Verify DNS: `dig yourdomain.com`
3. Check ingress: `kubectl describe ingress hbg-ingress`
4. Review certificate: `kubectl describe certificate hbg-cert`
5. Check Let's Encrypt rate limits: [Rate Limits](https://letsencrypt.org/docs/rate-limits/)

---

**Last Updated:** $(date)
**Version:** 1.0

