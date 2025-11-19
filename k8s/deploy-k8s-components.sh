#!/bin/bash

# Kubernetes Components Installation Script for Azure AKS
# This script installs NGINX Ingress Controller, cert-manager, and trust-manager
#
# Usage: ./deploy-k8s-components.sh

set -e  # Exit on error

echo "============================================"
echo "Installing Kubernetes Components"
echo "============================================"

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "ERROR: kubectl is not installed. Please install it first."
    exit 1
fi

# Check cluster connection
if ! kubectl cluster-info &> /dev/null; then
    echo "ERROR: Cannot connect to Kubernetes cluster."
    echo "Please run: az aks get-credentials --resource-group <rg> --name <cluster>"
    exit 1
fi

# ============================================
# Install NGINX Ingress Controller
# ============================================
echo ""
echo "Installing NGINX Ingress Controller..."
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml

echo "Waiting for ingress controller to be ready..."
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=300s

# Get external IP
echo ""
echo "Ingress Controller Status:"
kubectl get service ingress-nginx-controller -n ingress-nginx

INGRESS_IP=$(kubectl get service ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "Pending...")
INGRESS_HOSTNAME=$(kubectl get service ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>/dev/null || echo "Pending...")

echo ""
echo "Ingress External IP: $INGRESS_IP"
echo "Ingress Hostname: $INGRESS_HOSTNAME"
echo ""
echo "NOTE: It may take a few minutes for the IP address to be assigned."
echo "Run this command to check again:"
echo "  kubectl get service ingress-nginx-controller -n ingress-nginx"

# ============================================
# Install cert-manager
# ============================================
echo ""
echo "Installing cert-manager..."
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.3/cert-manager.yaml

echo "Waiting for cert-manager to be ready..."
kubectl wait --for=condition=ready pod \
  --all -n cert-manager \
  --timeout=300s

# ============================================
# Install trust-manager
# ============================================
echo ""
echo "Installing trust-manager..."
kubectl apply -f https://github.com/cert-manager/trust-manager/releases/download/v0.9.1/trust-manager.yaml

echo "Waiting for trust-manager to be ready..."
kubectl wait --for=condition=ready pod \
  --all -n cert-manager-system \
  --timeout=300s

echo ""
echo "============================================"
echo "Kubernetes Components Installed!"
echo "============================================"
echo ""
echo "Next steps:"
echo "1. Update Let's Encrypt issuer with your email:"
echo "   Edit: k8s/cert-manager/letsencrypt-issuer.yaml"
echo "   Replace: YOUR_EMAIL@example.com"
echo ""
echo "2. Apply Let's Encrypt issuer:"
echo "   kubectl apply -f k8s/cert-manager/letsencrypt-issuer.yaml"
echo ""
echo "3. Configure DNS records in Porkbun:"
echo "   Point subdomains to: $INGRESS_IP"
echo "   (Wait for IP to be assigned if it shows 'Pending')"
echo ""
echo "4. Update YAML files with your domain and ACR server"
echo "5. Build and push Docker images"
echo "6. Deploy Kubernetes resources"
echo "============================================"




