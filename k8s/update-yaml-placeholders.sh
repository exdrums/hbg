#!/bin/bash

# Script to update all Kubernetes YAML files with actual values
# This replaces placeholders with your Docker Hub username and domain name
#
# Usage:
#   export DOCKER_HUB_USERNAME="myname"
#   export DOMAIN_NAME="yourdomain.com"
#   ./update-yaml-placeholders.sh

set -e  # Exit on error
DOCKER_HUB_USERNAME="myname"
DOMAIN_NAME="hbg.lol" 

# Check if variables are set
if [ -z "$DOCKER_HUB_USERNAME" ] || [ -z "$DOMAIN_NAME" ]; then
    echo "ERROR: Required environment variables not set!"
    echo ""
    echo "Usage:"
    echo "  export DOCKER_HUB_USERNAME=\"myname\""
    echo "  export DOMAIN_NAME=\"yourdomain.com\""
    echo "  ./update-yaml-placeholders.sh"
    exit 1
fi

echo "============================================"
echo "Updating YAML files with actual values"
echo "============================================"
echo "Docker Hub Username: $DOCKER_HUB_USERNAME"
echo "Domain Name: $DOMAIN_NAME"
echo ""

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Update DOCKER_HUB_USERNAME in deployment files
echo "Updating image references in deployment files..."
find . -name "*.yaml" -type f -exec sed -i.bak "s|DOCKER_HUB_USERNAME|$DOCKER_HUB_USERNAME|g" {} \;

# Update YOUR_DOMAIN_NAME in all YAML files
echo "Updating domain names in YAML files..."
find . -name "*.yaml" -type f -exec sed -i.bak "s|YOUR_DOMAIN_NAME|$DOMAIN_NAME|g" {} \;

# Clean up backup files
echo "Cleaning up backup files..."
find . -name "*.bak" -type f -delete

echo ""
echo "============================================"
echo "YAML files updated successfully!"
echo "============================================"
echo ""
echo "Updated files:"
echo "- All deployment files (image references: $DOCKER_HUB_USERNAME/hbg-*:latest)"
echo "- ConfigMap (domain URLs)"
echo "- Ingress files (host rules and TLS)"
echo "- Certificate files (domain names)"
echo ""
echo "Next steps:"
echo "1. Review the updated files"
echo "2. Update Let's Encrypt issuer email:"
echo "   Edit: k8s/cert-manager/letsencrypt-issuer.yaml"
echo "   Replace: YOUR_EMAIL@example.com"
echo "3. Login to Docker Hub: docker login"
echo "4. Build and push Docker images: ./build-and-push-images.sh"
echo "5. If using private repos, create secret: ./setup-dockerhub-secret.sh"
echo "6. Deploy Kubernetes resources: ./deploy-k8s-resources.sh"
echo "============================================"



