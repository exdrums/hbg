#!/bin/bash

# TLS Setup Script for Azure Kubernetes Service
# This script helps set up TLS certificates using Let's Encrypt and cert-manager

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
EMAIL="${LETSENCRYPT_EMAIL:-}"
DOMAIN="${DOMAIN_NAME:-}"
INGRESS_NAMESPACE="ingress-nginx"
CERT_MANAGER_VERSION="v1.13.3"

echo -e "${GREEN}=== TLS Setup Script for Azure Kubernetes Service ===${NC}\n"

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}Error: kubectl is not installed${NC}"
    exit 1
fi

if ! kubectl cluster-info &> /dev/null; then
    echo -e "${RED}Error: Cannot connect to Kubernetes cluster${NC}"
    exit 1
fi

echo -e "${GREEN}✓ kubectl is installed and connected${NC}"

# Get email if not set
if [ -z "$EMAIL" ]; then
    echo -e "\n${YELLOW}Enter your email address for Let's Encrypt notifications:${NC}"
    read -r EMAIL
fi

if [ -z "$EMAIL" ]; then
    echo -e "${RED}Error: Email address is required${NC}"
    exit 1
fi

# Get domain if not set
if [ -z "$DOMAIN" ]; then
    echo -e "\n${YELLOW}Enter your domain name (e.g., hbg.lol):${NC}"
    read -r DOMAIN
fi

if [ -z "$DOMAIN" ]; then
    echo -e "${RED}Error: Domain name is required${NC}"
    exit 1
fi

echo -e "\n${GREEN}Configuration:${NC}"
echo "  Email: $EMAIL"
echo "  Domain: $DOMAIN"
echo ""

# Step 1: Install cert-manager
echo -e "${YELLOW}Step 1: Installing cert-manager...${NC}"
if kubectl get namespace cert-manager &> /dev/null; then
    echo -e "${GREEN}✓ cert-manager namespace already exists${NC}"
else
    kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/${CERT_MANAGER_VERSION}/cert-manager.yaml
    echo -e "${GREEN}✓ cert-manager installed${NC}"
fi

echo -e "${YELLOW}Waiting for cert-manager to be ready...${NC}"
kubectl wait --for=condition=ready pod \
  -l app.kubernetes.io/instance=cert-manager \
  -n cert-manager \
  --timeout=300s || {
    echo -e "${RED}Error: cert-manager failed to start${NC}"
    exit 1
}

echo -e "${GREEN}✓ cert-manager is ready${NC}\n"

# Step 2: Create Let's Encrypt issuers
echo -e "${YELLOW}Step 2: Creating Let's Encrypt issuers...${NC}"

# Create staging issuer
cat > /tmp/letsencrypt-staging-issuer.yaml <<EOF
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-staging
spec:
  acme:
    server: https://acme-staging-v02.api.letsencrypt.org/directory
    email: ${EMAIL}
    privateKeySecretRef:
      name: letsencrypt-staging-key
    solvers:
    - http01:
        ingress:
          class: nginx
EOF

# Create production issuer
cat > /tmp/letsencrypt-prod-issuer.yaml <<EOF
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory
    email: ${EMAIL}
    privateKeySecretRef:
      name: letsencrypt-prod-key
    solvers:
    - http01:
        ingress:
          class: nginx
EOF

kubectl apply -f /tmp/letsencrypt-staging-issuer.yaml
kubectl apply -f /tmp/letsencrypt-prod-issuer.yaml

echo -e "${GREEN}✓ Let's Encrypt issuers created${NC}\n"

# Step 3: Get ingress IP
echo -e "${YELLOW}Step 3: Getting ingress controller IP...${NC}"
INGRESS_IP=$(kubectl get service ingress-nginx-controller -n ${INGRESS_NAMESPACE} -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || \
             kubectl get service ingress-nginx-controller -n ${INGRESS_NAMESPACE} -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null)

if [ -z "$INGRESS_IP" ]; then
    echo -e "${RED}Error: Could not get ingress controller IP${NC}"
    echo -e "${YELLOW}Please ensure NGINX Ingress Controller is installed${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Ingress IP: ${INGRESS_IP}${NC}\n"

# Step 4: DNS instructions
echo -e "${YELLOW}Step 4: DNS Configuration Required${NC}"
echo -e "${GREEN}Please configure the following DNS A records pointing to ${INGRESS_IP}:${NC}"
echo ""
echo "  sts.${DOMAIN}        → ${INGRESS_IP}"
echo "  admin.${DOMAIN}       → ${INGRESS_IP}"
echo "  adminspa.${DOMAIN}   → ${INGRESS_IP}"
echo "  adminapi.${DOMAIN}   → ${INGRESS_IP}"
echo "  spa.${DOMAIN}        → ${INGRESS_IP}"
echo "  constructor.${DOMAIN} → ${INGRESS_IP}"
echo "  filesapi.${DOMAIN}   → ${INGRESS_IP}"
echo "  n8n.${DOMAIN}        → ${INGRESS_IP}"
echo "  monitoring.${DOMAIN} → ${INGRESS_IP}"
echo ""
echo -e "${YELLOW}Or use a wildcard record:${NC}"
echo "  *.${DOMAIN} → ${INGRESS_IP}"
echo ""

# Verify DNS
echo -e "${YELLOW}Waiting for DNS configuration...${NC}"
echo -e "Press Enter when DNS records are configured and propagated (usually 5-60 minutes)"
read -r

# Step 5: Verify DNS
echo -e "\n${YELLOW}Step 5: Verifying DNS configuration...${NC}"
DNS_OK=true

for subdomain in sts admin adminspa adminapi spa; do
    RESOLVED=$(dig +short ${subdomain}.${DOMAIN} | head -n1)
    if [ "$RESOLVED" = "$INGRESS_IP" ]; then
        echo -e "${GREEN}✓ ${subdomain}.${DOMAIN} → ${RESOLVED}${NC}"
    else
        echo -e "${RED}✗ ${subdomain}.${DOMAIN} → ${RESOLVED} (expected ${INGRESS_IP})${NC}"
        DNS_OK=false
    fi
done

if [ "$DNS_OK" = false ]; then
    echo -e "\n${RED}Warning: Some DNS records are not correctly configured${NC}"
    echo -e "${YELLOW}Please verify DNS configuration before proceeding${NC}"
    echo -e "Press Enter to continue anyway, or Ctrl+C to exit"
    read -r
fi

# Step 6: Verify ingress configuration
echo -e "\n${YELLOW}Step 6: Verifying ingress configuration...${NC}"
if kubectl get ingress hbg-ingress &> /dev/null; then
    ISSUER=$(kubectl get ingress hbg-ingress -o jsonpath='{.metadata.annotations.cert-manager\.io/cluster-issuer}')
    if [ "$ISSUER" = "letsencrypt-prod" ] || [ "$ISSUER" = "letsencrypt-staging" ]; then
        echo -e "${GREEN}✓ Ingress is configured with cert-manager issuer: ${ISSUER}${NC}"
    else
        echo -e "${YELLOW}⚠ Ingress issuer is: ${ISSUER}${NC}"
        echo -e "${YELLOW}  Expected: letsencrypt-prod or letsencrypt-staging${NC}"
        echo -e "${YELLOW}  Please update hbg-ingress.yaml and apply it${NC}"
    fi
else
    echo -e "${YELLOW}⚠ Ingress 'hbg-ingress' not found${NC}"
    echo -e "${YELLOW}  Please ensure ingress is deployed${NC}"
fi

# Step 7: Check certificates
echo -e "\n${YELLOW}Step 7: Checking certificate status...${NC}"
sleep 5
kubectl get certificates

echo -e "\n${GREEN}=== Setup Complete! ===${NC}\n"
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Monitor certificate issuance:"
echo "   kubectl get certificates --watch"
echo ""
echo "2. Check certificate details:"
echo "   kubectl describe certificate hbg-cert"
echo ""
echo "3. Test HTTPS endpoints:"
echo "   curl -I https://sts.${DOMAIN}"
echo ""
echo "4. View cert-manager logs if issues occur:"
echo "   kubectl logs -n cert-manager -l app=cert-manager"
echo ""
echo -e "${GREEN}Certificates will auto-renew 30 days before expiration.${NC}"

# Cleanup
rm -f /tmp/letsencrypt-staging-issuer.yaml /tmp/letsencrypt-prod-issuer.yaml

