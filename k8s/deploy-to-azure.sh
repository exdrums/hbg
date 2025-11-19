#!/bin/bash

# Azure Kubernetes Service Deployment Script for HBG
# This script automates the Azure infrastructure setup and deployment process
#
# Usage:
#   1. Set the variables below
#   2. Run: ./deploy-to-azure.sh

set -e  # Exit on error

# ============================================
# CONFIGURATION - UPDATE THESE VALUES
# ============================================

# Azure Configuration
RESOURCE_GROUP="hbg"
LOCATION="polandcentral"  # Choose your preferred Azure region
AKS_CLUSTER_NAME="hug-cluster"
# Note: Using Docker Hub instead of ACR - no ACR_NAME needed
NODE_COUNT=1
NODE_VM_SIZE="Standard_B2s"  # 2 vCPU, 4GB RAM - suitable for testing

# Domain Configuration
DOMAIN_NAME="hbg.lol"  # REPLACE with your actual Porkbun domain
EMAIL="houbirg55@gmail.com"  # REPLACE with your email for Let's Encrypt

# ============================================
# PHASE 1: Azure Infrastructure Setup
# ============================================

echo "============================================"
echo "Phase 1: Azure Infrastructure Setup"
echo "============================================"

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "ERROR: Azure CLI is not installed. Please install it first."
    echo "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Check if logged in to Azure
echo "Checking Azure login status..."
if ! az account show &> /dev/null; then
    echo "Please log in to Azure..."
    az login
fi

# Set subscription (optional - uncomment and set if you have multiple subscriptions)
# az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Create Resource Group
echo "Creating resource group: $RESOURCE_GROUP"
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION

# Note: Using Docker Hub instead of Azure Container Registry (ACR)
# This eliminates ACR costs and simplifies the setup
echo "Using Docker Hub for container images (no ACR setup needed)"

# Create AKS Cluster
echo "Creating AKS cluster: $AKS_CLUSTER_NAME"
echo "This may take 10-15 minutes..."
az aks create \
  --resource-group $RESOURCE_GROUP \
  --name $AKS_CLUSTER_NAME \
  --node-count $NODE_COUNT \
  --node-vm-size $NODE_VM_SIZE \
  --enable-managed-identity \
  --network-plugin azure \
  --enable-addons monitoring \
  --generate-ssh-keys \
  --location $LOCATION

# Get AKS credentials
echo "Getting AKS credentials..."
az aks get-credentials \
  --resource-group $RESOURCE_GROUP \
  --name $AKS_CLUSTER_NAME \
  --overwrite-existing

# Verify cluster connection
echo "Verifying cluster connection..."
kubectl get nodes

echo ""
echo "============================================"
echo "Phase 1 Complete!"
echo "============================================"
echo "Resource Group: $RESOURCE_GROUP"
echo "AKS Cluster: $AKS_CLUSTER_NAME"
echo "Container Registry: Docker Hub (myname)"
echo ""
echo "Next steps:"
echo "1. Update Kubernetes YAML files with DOCKER_HUB_USERNAME and DOMAIN_NAME"
echo "2. Run: ./deploy-k8s-components.sh"
echo "3. Login to Docker Hub: docker login"
echo "4. Build and push Docker images: ./build-and-push-images.sh"
echo "5. If using private repos, create secret: ./setup-dockerhub-secret.sh"
echo "6. Deploy Kubernetes resources: ./deploy-k8s-resources.sh"
echo "============================================"



