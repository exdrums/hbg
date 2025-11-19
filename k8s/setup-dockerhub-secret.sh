#!/bin/bash

# Setup Docker Hub Image Pull Secret for Kubernetes
# This script creates a Kubernetes secret for pulling images from private Docker Hub repositories
#
# Usage:
#   export DOCKER_HUB_USERNAME="myname"
#   export DOCKER_HUB_PASSWORD="your-password-or-token"
#   export DOCKER_HUB_EMAIL="your-email@example.com"
#   ./setup-dockerhub-secret.sh

set -e  # Exit on error

# Default values (can be overridden by environment variables)
DOCKER_HUB_USERNAME="${DOCKER_HUB_USERNAME:-myname}"
DOCKER_HUB_EMAIL="${DOCKER_HUB_EMAIL:-}"

# Check if required variables are set
if [ -z "$DOCKER_HUB_USERNAME" ] || [ -z "$DOCKER_HUB_PASSWORD" ]; then
    echo "ERROR: Required environment variables not set!"
    echo ""
    echo "Usage:"
    echo "  export DOCKER_HUB_USERNAME=\"myname\""
    echo "  export DOCKER_HUB_PASSWORD=\"your-password-or-token\""
    echo "  export DOCKER_HUB_EMAIL=\"your-email@example.com\"  # Optional"
    echo "  ./setup-dockerhub-secret.sh"
    echo ""
    echo "Note: For better security, use a Docker Hub access token instead of password:"
    echo "  1. Go to Docker Hub → Account Settings → Security → New Access Token"
    echo "  2. Create a token with read permissions"
    echo "  3. Use the token as DOCKER_HUB_PASSWORD"
    exit 1
fi

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "ERROR: kubectl is not installed or not in PATH"
    exit 1
fi

# Check cluster connection
if ! kubectl cluster-info &> /dev/null; then
    echo "ERROR: Cannot connect to Kubernetes cluster."
    echo "Please ensure kubectl is configured correctly."
    exit 1
fi

echo "============================================"
echo "Setting up Docker Hub Image Pull Secret"
echo "============================================"
echo "Docker Hub Username: $DOCKER_HUB_USERNAME"
echo "Docker Hub Email: ${DOCKER_HUB_EMAIL:-not provided}"
echo ""

# Namespaces where secrets should be created
NAMESPACES=("default" "monitoring")

# Create secret in each namespace
for NAMESPACE in "${NAMESPACES[@]}"; do
    echo "Creating secret in namespace: $NAMESPACE"
    
    # Check if namespace exists, create if not
    if ! kubectl get namespace "$NAMESPACE" &> /dev/null; then
        echo "  Creating namespace: $NAMESPACE"
        kubectl create namespace "$NAMESPACE"
    fi
    
    # Delete existing secret if it exists
    if kubectl get secret dockerhub-secret -n "$NAMESPACE" &> /dev/null; then
        echo "  Deleting existing secret..."
        kubectl delete secret dockerhub-secret -n "$NAMESPACE"
    fi
    
    # Create the secret
    if [ -n "$DOCKER_HUB_EMAIL" ]; then
        kubectl create secret docker-registry dockerhub-secret \
            --docker-server=https://index.docker.io/v1/ \
            --docker-username="$DOCKER_HUB_USERNAME" \
            --docker-password="$DOCKER_HUB_PASSWORD" \
            --docker-email="$DOCKER_HUB_EMAIL" \
            --namespace="$NAMESPACE"
    else
        kubectl create secret docker-registry dockerhub-secret \
            --docker-server=https://index.docker.io/v1/ \
            --docker-username="$DOCKER_HUB_USERNAME" \
            --docker-password="$DOCKER_HUB_PASSWORD" \
            --namespace="$NAMESPACE"
    fi
    
    echo "  ✓ Secret created in namespace: $NAMESPACE"
done

echo ""
echo "============================================"
echo "Docker Hub Secret Setup Complete!"
echo "============================================"
echo ""
echo "Secrets created in namespaces: ${NAMESPACES[*]}"
echo ""
echo "✓ All deployment YAML files already include imagePullSecrets"
echo "  The secret 'dockerhub-secret' is configured in all deployments"
echo ""
echo "Note: If using PUBLIC repositories, you can remove imagePullSecrets"
echo "      from deployment YAML files (they're marked with comments)"
echo ""
echo "To verify secrets:"
echo "  kubectl get secret dockerhub-secret -n default"
echo "  kubectl get secret dockerhub-secret -n monitoring"
echo ""
echo "To test image pull:"
echo "  kubectl run test-pull --image=$DOCKER_HUB_USERNAME/hbg-sts:latest --rm -it --restart=Never"
echo "============================================"

