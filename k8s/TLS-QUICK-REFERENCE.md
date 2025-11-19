# TLS Quick Reference Guide

Quick commands and tips for managing TLS certificates in your Azure Kubernetes cluster.

## Quick Setup

```bash
# Automated setup
export LETSENCRYPT_EMAIL="your-email@example.com"
export DOMAIN_NAME="yourdomain.com"
./setup-tls.sh
```

## Common Commands

### Check Certificate Status
```bash
# List all certificates
kubectl get certificates

# Detailed certificate info
kubectl describe certificate hbg-cert

# Check certificate expiration
kubectl get certificate hbg-cert -o jsonpath='{.status.notAfter}'
```

### Check Certificate Requests
```bash
# List certificate requests
kubectl get certificaterequests

# Describe a request
kubectl describe certificaterequest <request-name>
```

### Check Challenges (HTTP-01 Validation)
```bash
# List challenges
kubectl get challenges

# Describe a challenge
kubectl describe challenge <challenge-name>
```

### Check Issuers
```bash
# List ClusterIssuers
kubectl get clusterissuer

# Check issuer status
kubectl describe clusterissuer letsencrypt-prod
kubectl describe clusterissuer letsencrypt-staging
```

### Check TLS Secrets
```bash
# List TLS secrets
kubectl get secrets | grep tls

# View certificate details
kubectl get secret hbg-tls-secret -o jsonpath='{.data.tls\.crt}' | base64 -d | openssl x509 -text -noout
```

### Cert-manager Logs
```bash
# Main cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager --tail=100

# Webhook logs
kubectl logs -n cert-manager -l app=webhook --tail=100

# CA injector logs
kubectl logs -n cert-manager -l app=cainjector --tail=100
```

## Troubleshooting

### Certificate Stuck in Pending
```bash
# Delete and let cert-manager recreate
kubectl delete certificate hbg-cert
kubectl delete secret hbg-tls-secret
# cert-manager will recreate automatically via ingress annotation
```

### Force Certificate Renewal
```bash
# Delete the secret and certificate
kubectl delete secret hbg-tls-secret
kubectl delete certificate hbg-cert

# cert-manager will recreate automatically
```

### Check DNS Configuration
```bash
# Get ingress IP
INGRESS_IP=$(kubectl get service ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "Ingress IP: $INGRESS_IP"

# Test DNS resolution
dig sts.yourdomain.com +short
nslookup sts.yourdomain.com
```

### Test HTTPS Endpoints
```bash
# Test with curl
curl -I https://sts.yourdomain.com
curl -v https://sts.yourdomain.com

# Test with openssl
openssl s_client -connect sts.yourdomain.com:443 -servername sts.yourdomain.com < /dev/null
```

### Verify Ingress Configuration
```bash
# Check ingress annotations
kubectl get ingress hbg-ingress -o yaml | grep cert-manager

# Verify ingress is using correct issuer
kubectl get ingress hbg-ingress -o jsonpath='{.metadata.annotations.cert-manager\.io/cluster-issuer}'
```

## Switching Between Staging and Production

### Use Staging (for Testing)
```bash
# Update ingress annotation
kubectl annotate ingress hbg-ingress cert-manager.io/cluster-issuer=letsencrypt-staging --overwrite

# Delete existing certificate to force recreation
kubectl delete certificate hbg-cert
kubectl delete secret hbg-tls-secret
```

### Use Production
```bash
# Update ingress annotation
kubectl annotate ingress hbg-ingress cert-manager.io/cluster-issuer=letsencrypt-prod --overwrite

# Delete existing certificate to force recreation
kubectl delete certificate hbg-cert
kubectl delete secret hbg-tls-secret
```

## Monitoring Certificate Health

### Check Certificate Expiration
```bash
# Get expiration date
kubectl get certificate hbg-cert -o jsonpath='{.status.notAfter}'

# Calculate days until expiration (macOS)
EXPIRY=$(kubectl get certificate hbg-cert -o jsonpath='{.status.notAfter}')
EXPIRY_EPOCH=$(date -j -f "%Y-%m-%dT%H:%M:%SZ" "$EXPIRY" +%s)
NOW_EPOCH=$(date +%s)
DAYS_LEFT=$(( ($EXPIRY_EPOCH - $NOW_EPOCH) / 86400 ))
echo "Certificate expires in $DAYS_LEFT days"
```

### Watch Certificate Status
```bash
# Watch certificates
kubectl get certificates --watch

# Watch certificate requests
kubectl get certificaterequests --watch

# Watch challenges
kubectl get challenges --watch
```

## Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| Certificate not issued | Check DNS, verify ingress IP, check cert-manager logs |
| Certificate stuck pending | Delete certificate and secret, let cert-manager recreate |
| Rate limit exceeded | Use staging issuer, wait for rate limit reset |
| DNS not resolving | Wait 5-60 minutes for propagation, verify DNS records |
| Wrong certificate | Check issuer annotation in ingress |
| Certificate expired | cert-manager auto-renews, but can force renewal by deleting secret |

## DNS Records Required

Point these subdomains to your ingress IP:

```
sts.yourdomain.com        → <INGRESS_IP>
admin.yourdomain.com      → <INGRESS_IP>
adminspa.yourdomain.com   → <INGRESS_IP>
adminapi.yourdomain.com   → <INGRESS_IP>
spa.yourdomain.com        → <INGRESS_IP>
constructor.yourdomain.com → <INGRESS_IP>
filesapi.yourdomain.com   → <INGRESS_IP>
n8n.yourdomain.com        → <INGRESS_IP>
monitoring.yourdomain.com → <INGRESS_IP>
```

Or use wildcard:
```
*.yourdomain.com → <INGRESS_IP>
```

## Let's Encrypt Rate Limits

- **50 certificates per registered domain per week**
- **5 duplicate certificates per week**
- **300 new orders per 3 hours**

**Solution**: Use staging issuer for testing to avoid hitting limits.

## Useful Links

- [Full TLS Setup Guide](./TLS-SETUP-GUIDE.md)
- [cert-manager Documentation](https://cert-manager.io/docs/)
- [Let's Encrypt Rate Limits](https://letsencrypt.org/docs/rate-limits/)
- [NGINX Ingress Controller](https://kubernetes.github.io/ingress-nginx/)

