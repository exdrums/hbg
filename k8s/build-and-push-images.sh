#!/bin/bash

# Build and Push Docker Images to Docker Hub
# This script builds all HBG Docker images and pushes them to Docker Hub
#
# Usage:
#   export DOCKER_HUB_USERNAME="myname"
#   ./build-and-push-images.sh

set -e  # Exit on error

# Check if variables are set
if [ -z "$DOCKER_HUB_USERNAME" ]; then
    echo "ERROR: DOCKER_HUB_USERNAME environment variable not set!"
    echo ""
    echo "Usage:"
    echo "  export DOCKER_HUB_USERNAME=\"myname\""
    echo "  ./build-and-push-images.sh"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "ERROR: Docker is not running. Please start Docker first."
    exit 1
fi

# Get the project root directory (assuming script is in k8s/)
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"
cd "$PROJECT_ROOT"

echo "============================================"
echo "Building and Pushing Docker Images"
echo "============================================"
echo "Docker Hub Username: $DOCKER_HUB_USERNAME"
echo "Project Root: $PROJECT_ROOT"
echo ""

# Check if logged in to Docker Hub
echo "Checking Docker Hub login status..."
if ! docker info | grep -q "Username"; then
    echo "⚠ Warning: Not logged in to Docker Hub"
    echo "Please run: docker login"
    echo "Then run this script again."
    exit 1
fi

echo "✓ Docker Hub login verified"

# Build and push images
# Note: Adjust Dockerfile paths based on your actual project structure

echo ""
echo "Building hbg-sts image..."
if [ -f "src/Services/API/Identity/Dockerfile" ]; then
    docker build -t $DOCKER_HUB_USERNAME/hbg-sts:latest -f src/Services/API/Identity/Dockerfile .
    docker push $DOCKER_HUB_USERNAME/hbg-sts:latest
    echo "✓ hbg-sts pushed"
else
    echo "⚠ Warning: Dockerfile not found for hbg-sts"
fi

echo ""
echo "Building hbg-admin image..."
if [ -f "src/Services/API/Identity/Dockerfile" ]; then
    docker build -t $DOCKER_HUB_USERNAME/hbg-admin:latest -f src/Services/API/Identity/Dockerfile .
    docker push $DOCKER_HUB_USERNAME/hbg-admin:latest
    echo "✓ hbg-admin pushed"
else
    echo "⚠ Warning: Dockerfile not found for hbg-admin"
fi

echo ""
echo "Building hbg-admin-api image..."
if [ -f "src/Services/API/Identity/API.Identity.Admin.Api/Dockerfile" ]; then
    docker build -t $DOCKER_HUB_USERNAME/hbg-admin-api:latest -f src/Services/API/Identity/API.Identity.Admin.Api/Dockerfile src/Services/API/Identity/
    docker push $DOCKER_HUB_USERNAME/hbg-admin-api:latest
    echo "✓ hbg-admin-api pushed"
else
    echo "⚠ Warning: Dockerfile not found for hbg-admin-api"
fi

echo ""
echo "Building hbg-admin-spa image..."
if [ -f "src/Web/Web.SPA/Dockerfile" ]; then
    docker build -t $DOCKER_HUB_USERNAME/hbg-admin-spa:latest -f src/Web/Web.SPA/Dockerfile .
    docker push $DOCKER_HUB_USERNAME/hbg-admin-spa:latest
    echo "✓ hbg-admin-spa pushed"
else
    echo "⚠ Warning: Dockerfile not found for hbg-admin-spa"
fi

echo ""
echo "Building hbg-spa image..."
if [ -f "src/Web/Web.SPA/Dockerfile" ]; then
    docker build -t $DOCKER_HUB_USERNAME/hbg-spa:latest -f src/Web/Web.SPA/Dockerfile .
    docker push $DOCKER_HUB_USERNAME/hbg-spa:latest
    echo "✓ hbg-spa pushed"
else
    echo "⚠ Warning: Dockerfile not found for hbg-spa"
fi

echo ""
echo "Building hbg-files-api image..."
if [ -f "src/Services/API/Files/Dockerfile" ]; then
    docker build -t $DOCKER_HUB_USERNAME/hbg-files-api:latest -f src/Services/API/Files/Dockerfile .
    docker push $DOCKER_HUB_USERNAME/hbg-files-api:latest
    echo "✓ hbg-files-api pushed"
else
    echo "⚠ Warning: Dockerfile not found for hbg-files-api"
fi

echo ""
echo "============================================"
echo "Build and Push Complete!"
echo "============================================"
echo ""
echo "Images pushed to Docker Hub: $DOCKER_HUB_USERNAME"
echo ""
echo "Next steps:"
echo "1. Verify images in Docker Hub:"
echo "   Visit: https://hub.docker.com/r/$DOCKER_HUB_USERNAME"
echo ""
echo "2. If using private repositories, create Kubernetes secret:"
echo "   ./setup-dockerhub-secret.sh"
echo ""
echo "3. Deploy Kubernetes resources:"
echo "   ./deploy-k8s-resources.sh"
echo "============================================"


