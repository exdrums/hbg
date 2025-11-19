#!/bin/bash

# Deploy Kubernetes Resources to AKS
# This script deploys all Kubernetes resources in the correct order
#
# Usage: ./deploy-k8s-resources.sh

set -e  # Exit on error

echo "============================================"
echo "Deploying Kubernetes Resources"
echo "============================================"

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Check cluster connection
if ! kubectl cluster-info &> /dev/null; then
    echo "ERROR: Cannot connect to Kubernetes cluster."
    echo "Please run: az aks get-credentials --resource-group <rg> --name <cluster>"
    exit 1
fi

# ============================================
# Step 1: Create Namespaces
# ============================================
echo ""
echo "Creating namespaces..."
kubectl apply -f uptime-kuma/namespace.yaml

# ============================================
# Step 2: Create Secrets
# ============================================
echo ""
echo "Creating secrets..."
kubectl apply -f hbg-secret.yaml

# Note: Docker Hub secret (dockerhub-secret) should be created separately
# if using private repositories. Run: ./setup-dockerhub-secret.sh

# ============================================
# Step 3: Create ConfigMaps
# ============================================
echo ""
echo "Creating ConfigMaps..."
kubectl apply -f hbg-configmap.yaml

# ============================================
# Step 4: Deploy Database Storage and Service
# ============================================
echo ""
echo "Deploying database storage..."
kubectl apply -f hbg-db-data.yaml

echo "Waiting for PVC to be bound..."
kubectl wait --for=jsonpath='{.status.phase}'=Bound pvc/hbg-db-pvc --timeout=60s || {
    echo "⚠ Warning: PVC not bound yet. Continuing..."
}

echo ""
echo "Deploying database..."
kubectl apply -f hbg-db.yaml

# ============================================
# Step 5: Deploy Files API Storage
# ============================================
echo ""
echo "Deploying Files API storage..."
kubectl apply -f hbg-files-storage.yaml

echo "Waiting for Files API PVC to be bound..."
kubectl wait --for=jsonpath='{.status.phase}'=Bound pvc/hbg-files-storage-pvc --timeout=60s || {
    echo "⚠ Warning: Files API PVC not bound yet. Continuing..."
}

# ============================================
# Step 6: Deploy Application Services
# ============================================
echo ""
echo "Deploying application services..."
kubectl apply -f hbg-sts.yaml
kubectl apply -f hbg-admin.yaml
kubectl apply -f hbg-admin-api.yaml
kubectl apply -f hbg-admin-spa.yaml
kubectl apply -f hbg-spa.yaml
kubectl apply -f hbg-files-api.yaml

# ============================================
# Step 7: Deploy Ingress
# ============================================
echo ""
echo "Deploying ingress..."
kubectl apply -f hbg-ingress.yaml

# ============================================
# Step 8: Deploy Monitoring (Uptime Kuma)
# ============================================
echo ""
echo "Deploying Uptime Kuma monitoring..."
kubectl apply -f uptime-kuma/storage.yaml
kubectl apply -f uptime-kuma/deployment.yaml
kubectl apply -f uptime-kuma/service.yaml
kubectl apply -f uptime-kuma/ingress.yaml

# ============================================
# Step 9: Wait for Deployments
# ============================================
echo ""
echo "Waiting for deployments to be ready..."
kubectl wait --for=condition=available --timeout=300s deployment/hbg-db || echo "⚠ Warning: hbg-db deployment not ready"
kubectl wait --for=condition=available --timeout=300s deployment/hbg-sts || echo "⚠ Warning: hbg-sts deployment not ready"
kubectl wait --for=condition=available --timeout=300s deployment/hbg-admin || echo "⚠ Warning: hbg-admin deployment not ready"
kubectl wait --for=condition=available --timeout=300s deployment/hbg-admin-api || echo "⚠ Warning: hbg-admin-api deployment not ready"
kubectl wait --for=condition=available --timeout=300s deployment/hbg-admin-spa || echo "⚠ Warning: hbg-admin-spa deployment not ready"
kubectl wait --for=condition=available --timeout=300s deployment/hbg-spa || echo "⚠ Warning: hbg-spa deployment not ready"
kubectl wait --for=condition=available --timeout=300s deployment/hbg-files-api || echo "⚠ Warning: hbg-files-api deployment not ready"
kubectl wait --for=condition=available --timeout=300s deployment/uptime-kuma -n monitoring || echo "⚠ Warning: uptime-kuma deployment not ready"

echo ""
echo "============================================"
echo "Deployment Complete!"
echo "============================================"
echo ""
echo "Checking pod status..."
kubectl get pods --all-namespaces

echo ""
echo "Checking services..."
kubectl get services --all-namespaces

echo ""
echo "Checking ingress..."
kubectl get ingress --all-namespaces

echo ""
echo "Checking certificates..."
kubectl get certificates --all-namespaces

echo ""
echo "============================================"
echo "Next steps:"
echo "1. Verify DNS records are configured in Porkbun"
echo "2. Check certificate status:"
echo "   kubectl describe certificate hbg-cert -n default"
echo "3. Test services:"
echo "   curl -k https://sts.YOUR_DOMAIN_NAME/health"
echo "============================================"


