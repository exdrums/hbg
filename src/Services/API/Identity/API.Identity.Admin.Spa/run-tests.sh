#!/bin/bash

# Identity Admin SPA Test Runner
# This script runs all tests for the Identity Admin SPA project

set -e

echo "==============================================="
echo "Identity Admin SPA - Test Runner"
echo "==============================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Get the directory where the script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to run Angular tests
run_angular_tests() {
    print_status "Running Angular unit tests..."
    cd "$SCRIPT_DIR/Client"

    if [ ! -d "node_modules" ]; then
        print_warning "node_modules not found. Installing dependencies..."
        npm install
    fi

    npm test -- --watch=false --browsers=ChromeHeadless

    if [ $? -eq 0 ]; then
        print_status "Angular tests passed ✓"
    else
        print_error "Angular tests failed ✗"
        return 1
    fi
}

# Function to run ASP.NET Core tests
run_dotnet_tests() {
    print_status "Running ASP.NET Core integration tests..."
    cd "$SCRIPT_DIR/../API.Identity.Admin.Spa.Tests"

    dotnet test --verbosity normal

    if [ $? -eq 0 ]; then
        print_status "ASP.NET Core tests passed ✓"
    else
        print_error "ASP.NET Core tests failed ✗"
        return 1
    fi
}

# Function to build Docker image
build_docker_image() {
    print_status "Building Docker image..."
    cd "$SCRIPT_DIR/../../../../../../"  # Navigate to repo root

    docker build -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
        -t exdrums/hbg-admin-spa:test \
        .

    if [ $? -eq 0 ]; then
        print_status "Docker image built successfully ✓"
    else
        print_error "Docker image build failed ✗"
        return 1
    fi
}

# Function to run Docker health check
test_docker_health() {
    print_status "Testing Docker container health check..."

    # Start container
    docker run -d --name hbg-admin-spa-test \
        -p 8080:80 \
        -e ASPNETCORE_ENVIRONMENT=Development \
        exdrums/hbg-admin-spa:test

    # Wait for container to be ready
    sleep 10

    # Test health endpoint
    HEALTH_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)

    # Cleanup
    docker stop hbg-admin-spa-test
    docker rm hbg-admin-spa-test

    if [ "$HEALTH_RESPONSE" = "200" ]; then
        print_status "Docker health check passed ✓"
    else
        print_error "Docker health check failed (HTTP $HEALTH_RESPONSE) ✗"
        return 1
    fi
}

# Main execution
main() {
    TEST_TYPE="${1:-all}"

    case "$TEST_TYPE" in
        angular)
            run_angular_tests
            ;;
        dotnet)
            run_dotnet_tests
            ;;
        docker)
            build_docker_image && test_docker_health
            ;;
        all)
            run_angular_tests && \
            run_dotnet_tests && \
            build_docker_image && \
            test_docker_health
            ;;
        *)
            echo "Usage: $0 {angular|dotnet|docker|all}"
            echo ""
            echo "  angular  - Run Angular unit tests"
            echo "  dotnet   - Run ASP.NET Core integration tests"
            echo "  docker   - Build and test Docker image"
            echo "  all      - Run all tests (default)"
            exit 1
            ;;
    esac

    if [ $? -eq 0 ]; then
        echo ""
        print_status "All tests completed successfully! ✓"
        exit 0
    else
        echo ""
        print_error "Some tests failed ✗"
        exit 1
    fi
}

main "$@"
