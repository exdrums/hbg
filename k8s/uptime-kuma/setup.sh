#!/bin/bash

# Uptime Kuma Setup Script for HBG Kubernetes Cluster
# This script helps deploy and configure Uptime Kuma monitoring solution

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if kubectl is available
check_kubectl() {
    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl not found. Please install kubectl first."
        exit 1
    fi
    print_success "kubectl is available"
}

# Function to check if cluster is accessible
check_cluster() {
    if ! kubectl cluster-info &> /dev/null; then
        print_error "Cannot connect to Kubernetes cluster. Please check your kubeconfig."
        exit 1
    fi
    print_success "Kubernetes cluster is accessible"
}

# Function to deploy Uptime Kuma
deploy_uptime_kuma() {
    print_info "Deploying Uptime Kuma to Kubernetes cluster..."

    # Create namespace
    print_info "Creating monitoring namespace..."
    kubectl apply -f namespace.yaml

    # Create PVC
    print_info "Creating PersistentVolumeClaim..."
    kubectl apply -f storage.yaml

    # Wait for PVC to be bound
    print_info "Waiting for PVC to be bound..."
    kubectl wait --for=jsonpath='{.status.phase}'=Bound pvc/uptime-kuma-pvc -n monitoring --timeout=60s || {
        print_warning "PVC is not bound yet. You may need to configure a storage provisioner."
    }

    # Deploy Uptime Kuma
    print_info "Creating Uptime Kuma deployment..."
    kubectl apply -f deployment.yaml

    # Create service
    print_info "Creating Uptime Kuma service..."
    kubectl apply -f service.yaml

    # Create ingress
    print_info "Creating Uptime Kuma ingress..."
    kubectl apply -f ingress.yaml

    print_success "Uptime Kuma deployed successfully!"
}

# Function to check deployment status
check_status() {
    print_info "Checking Uptime Kuma deployment status..."

    echo ""
    print_info "Namespace:"
    kubectl get namespace monitoring 2>/dev/null || print_warning "Namespace 'monitoring' not found"

    echo ""
    print_info "PersistentVolumeClaim:"
    kubectl get pvc -n monitoring 2>/dev/null || print_warning "No PVCs found in monitoring namespace"

    echo ""
    print_info "Deployment:"
    kubectl get deployment -n monitoring 2>/dev/null || print_warning "No deployments found in monitoring namespace"

    echo ""
    print_info "Pods:"
    kubectl get pods -n monitoring 2>/dev/null || print_warning "No pods found in monitoring namespace"

    echo ""
    print_info "Service:"
    kubectl get service -n monitoring 2>/dev/null || print_warning "No services found in monitoring namespace"

    echo ""
    print_info "Ingress:"
    kubectl get ingress -n monitoring 2>/dev/null || print_warning "No ingress found in monitoring namespace"
}

# Function to wait for deployment to be ready
wait_for_ready() {
    print_info "Waiting for Uptime Kuma to be ready..."
    kubectl wait --for=condition=available --timeout=300s deployment/uptime-kuma -n monitoring || {
        print_error "Deployment did not become ready in time"
        print_info "Check pod logs with: kubectl logs -n monitoring -l app=uptime-kuma"
        return 1
    }
    print_success "Uptime Kuma is ready!"
}

# Function to show access information
show_access_info() {
    echo ""
    print_info "========================================="
    print_info "Uptime Kuma Access Information"
    print_info "========================================="
    echo ""
    print_info "URL: https://monitoring.hbg.lol"
    echo ""
    print_warning "Make sure to add 'monitoring.hbg.lol' to your /etc/hosts file:"
    print_info "  sudo sh -c 'echo \"<YOUR_CLUSTER_IP> monitoring.hbg.lol\" >> /etc/hosts'"
    echo ""
    print_info "For initial setup, access the web UI and create an admin account."
    echo ""
    print_info "After setup, you can manually configure monitors using the provided"
    print_info "monitors-config.json file as a reference."
    echo ""
    print_info "========================================="
}

# Function to show logs
show_logs() {
    print_info "Showing Uptime Kuma logs (last 50 lines)..."
    kubectl logs -n monitoring -l app=uptime-kuma --tail=50 || {
        print_error "Could not retrieve logs. Is the pod running?"
    }
}

# Function to delete Uptime Kuma
delete_uptime_kuma() {
    print_warning "This will delete Uptime Kuma and all its data!"
    read -p "Are you sure? (yes/no): " confirmation

    if [ "$confirmation" != "yes" ]; then
        print_info "Deletion cancelled"
        return
    fi

    print_info "Deleting Uptime Kuma resources..."
    kubectl delete -f ingress.yaml --ignore-not-found=true
    kubectl delete -f service.yaml --ignore-not-found=true
    kubectl delete -f deployment.yaml --ignore-not-found=true
    kubectl delete -f storage.yaml --ignore-not-found=true

    read -p "Do you want to delete the monitoring namespace? (yes/no): " delete_ns
    if [ "$delete_ns" = "yes" ]; then
        kubectl delete -f namespace.yaml --ignore-not-found=true
        print_success "Namespace deleted"
    fi

    print_success "Uptime Kuma deleted successfully!"
}

# Function to restart deployment
restart_deployment() {
    print_info "Restarting Uptime Kuma deployment..."
    kubectl rollout restart deployment/uptime-kuma -n monitoring
    print_success "Restart initiated"
    wait_for_ready
}

# Function to port-forward for local access
port_forward() {
    print_info "Starting port-forward to Uptime Kuma..."
    print_info "Access Uptime Kuma at: http://localhost:3001"
    print_warning "Press Ctrl+C to stop port forwarding"
    kubectl port-forward -n monitoring svc/uptime-kuma-service 3001:3001
}

# Function to show help
show_help() {
    echo "Uptime Kuma Setup Script for HBG"
    echo ""
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  deploy          Deploy Uptime Kuma to Kubernetes"
    echo "  status          Check deployment status"
    echo "  wait            Wait for deployment to be ready"
    echo "  logs            Show Uptime Kuma logs"
    echo "  restart         Restart Uptime Kuma deployment"
    echo "  delete          Delete Uptime Kuma deployment"
    echo "  port-forward    Port-forward to local machine (access at localhost:3001)"
    echo "  access          Show access information"
    echo "  help            Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 deploy          # Deploy Uptime Kuma"
    echo "  $0 status          # Check status"
    echo "  $0 logs            # View logs"
    echo "  $0 port-forward    # Access locally"
}

# Main script logic
main() {
    case "${1:-help}" in
        deploy)
            check_kubectl
            check_cluster
            deploy_uptime_kuma
            wait_for_ready
            show_access_info
            ;;
        status)
            check_kubectl
            check_cluster
            check_status
            ;;
        wait)
            check_kubectl
            check_cluster
            wait_for_ready
            ;;
        logs)
            check_kubectl
            check_cluster
            show_logs
            ;;
        restart)
            check_kubectl
            check_cluster
            restart_deployment
            ;;
        delete)
            check_kubectl
            check_cluster
            delete_uptime_kuma
            ;;
        port-forward)
            check_kubectl
            check_cluster
            port_forward
            ;;
        access)
            show_access_info
            ;;
        help|--help|-h)
            show_help
            ;;
        *)
            print_error "Unknown command: $1"
            echo ""
            show_help
            exit 1
            ;;
    esac
}

# Run main function
main "$@"
