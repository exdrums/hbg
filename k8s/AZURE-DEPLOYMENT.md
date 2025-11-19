# Azure Kubernetes Service Deployment Guide for HBG

This guide provides step-by-step instructions to deploy your HBG Kubernetes cluster to Azure Kubernetes Service (AKS), configure domain binding with Porkbun, and set up appropriate storage and compute resources for a testing environment.

## Quick Start

1. **Set up Azure infrastructure**: `./deploy-to-azure.sh`
2. **Install Kubernetes components**: `./deploy-k8s-components.sh`
3. **Update YAML files**: `./update-yaml-placeholders.sh`
4. **Login to Docker Hub**: `docker login`
5. **Build and push images**: `./build-and-push-images.sh`
6. **Create Docker Hub secret** (if using private repos): `./setup-dockerhub-secret.sh`
7. **Deploy resources**: `./deploy-k8s-resources.sh`

## Prerequisites

- [ ] Azure subscription active and logged in
- [ ] Domain name registered at Porkbun.com
- [ ] Docker Hub account created (username: `myname`)
- [ ] Azure CLI installed (`az --version`)
- [ ] kubectl installed (`kubectl version --client`)
- [ ] Docker installed and running
- [ ] Basic knowledge of Kubernetes and Azure

## Step-by-Step Deployment

### Step 1: Azure Infrastructure Setup

Run the Azure infrastructure setup script:

```bash
cd k8s
./deploy-to-azure.sh
```

This script will:
- Create a resource group
- Create AKS cluster with appropriate VM sizes for testing
- Set up kubectl credentials

**Note**: Update the variables in `deploy-to-azure.sh` before running:
- `RESOURCE_GROUP`: Your resource group name
- `LOCATION`: Azure region (e.g., "eastus")
- `AKS_CLUSTER_NAME`: Your AKS cluster name
- `NODE_COUNT`: Number of nodes (2 recommended for testing)
- `NODE_VM_SIZE`: VM size (Standard_B2s recommended for testing)

**Note**: This deployment uses Docker Hub instead of Azure Container Registry (ACR), which eliminates ACR costs and simplifies the setup.

### Step 2: Install Kubernetes Components

Install NGINX Ingress Controller, cert-manager, and trust-manager:

```bash
./deploy-k8s-components.sh
```

This will:
- Install NGINX Ingress Controller
- Install cert-manager for TLS certificate management
- Install trust-manager for CA certificate injection
- Display the ingress controller IP address (needed for DNS configuration)

**Important**: Save the ingress controller IP address - you'll need it for DNS configuration.

### Step 3: Configure TLS Certificates

**For a complete TLS setup guide, see [TLS-SETUP-GUIDE.md](./TLS-SETUP-GUIDE.md)**

Quick setup options:

**Option A: Automated Setup Script (Recommended)**
```bash
export LETSENCRYPT_EMAIL="your-email@example.com"
export DOMAIN_NAME="yourdomain.com"
./setup-tls.sh
```

**Option B: Manual Setup**
1. Edit `cert-manager/letsencrypt-issuer.yaml` and replace `YOUR_EMAIL@example.com` with your actual email
2. Apply the issuers:
```bash
kubectl apply -f cert-manager/letsencrypt-staging-issuer.yaml  # For testing
kubectl apply -f cert-manager/letsencrypt-issuer.yaml           # For production
```

**Important**: The ingress is already configured to use `letsencrypt-prod`. Ensure DNS records are configured before deploying.

### Step 4: Update YAML Files with Actual Values

Replace placeholders in YAML files with your Docker Hub username and domain name:

```bash
export DOCKER_HUB_USERNAME="myname"  # Your Docker Hub username
export DOMAIN_NAME="yourdomain.com"  # Your Porkbun domain
./update-yaml-placeholders.sh
```

This script will:
- Replace `DOCKER_HUB_USERNAME` with your Docker Hub username in all deployment files (images will be `myname/hbg-*:latest`)
- Replace `YOUR_DOMAIN_NAME` with your actual domain in all YAML files

### Step 5: Configure DNS Records in Porkbun

1. Log in to Porkbun.com
2. Navigate to DNS settings for your domain
3. Get the ingress controller IP address:

```bash
kubectl get service ingress-nginx-controller -n ingress-nginx
```

4. Add the following A records (replace `YOUR_DOMAIN` with your domain):
   - `sts` → `<INGRESS_IP>`
   - `admin` → `<INGRESS_IP>`
   - `adminspa` → `<INGRESS_IP>`
   - `spa` → `<INGRESS_IP>`
   - `adminapi` → `<INGRESS_IP>`
   - `filesapi` → `<INGRESS_IP>`
   - `monitoring` → `<INGRESS_IP>`

   Or use a wildcard record:
   - `*` → `<INGRESS_IP>`

**Note**: DNS propagation can take 5-60 minutes. Verify with:
```bash
dig sts.yourdomain.com
nslookup sts.yourdomain.com
```

### Step 6: Set Up Azure Storage for Files API

The Files API requires persistent storage for uploaded images and files. Azure Kubernetes Service automatically provisions storage when you create a PersistentVolumeClaim (PVC), but you may want to configure specific storage options.

#### Option 1: Use Default Azure-Managed Storage (Recommended for Testing)

The default configuration uses Azure-managed disks with the `managed-csi` storage class. This is automatically provisioned when you deploy `hbg-files-storage.yaml`. No additional setup is required.

**Storage Details:**
- **Type**: Azure-managed disk (Standard SSD)
- **Size**: 20GB (configurable in `hbg-files-storage.yaml`)
- **Access Mode**: ReadWriteOnce (single pod access)
- **Cost**: ~$2/month for 20GB Standard SSD

#### Option 2: Use Azure Files for ReadWriteMany Access

If you need multiple pods to access the same storage simultaneously (for scaling), use Azure Files:

1. **Create Azure Storage Account** (if not already created):
```bash
az storage account create \
  --resource-group <RESOURCE_GROUP> \
  --name <STORAGE_ACCOUNT_NAME> \
  --location <LOCATION> \
  --sku Standard_LRS
```

2. **Create Storage Class for Azure Files**:
Create a file `azure-files-sc.yaml`:
```yaml
apiVersion: storage.k8s.io/v1
kind: StorageClass
metadata:
  name: azurefile-csi
provisioner: file.csi.azure.com
parameters:
  skuName: Standard_LRS
allowVolumeExpansion: true
```

Apply it:
```bash
kubectl apply -f azure-files-sc.yaml
```

3. **Update `hbg-files-storage.yaml`**:
Change the `accessModes` to `ReadWriteMany` and add `storageClassName: azurefile-csi`:
```yaml
spec:
  accessModes:
  - ReadWriteMany
  storageClassName: azurefile-csi
  resources:
    requests:
      storage: 20Gi
```

**Note**: Azure Files costs more (~$0.06/GB/month) but supports ReadWriteMany access.

#### Verify Storage Setup

After deploying, verify the PVC is bound:
```bash
kubectl get pvc hbg-files-storage-pvc
kubectl describe pvc hbg-files-storage-pvc
```

The PVC should show `Status: Bound` and display the Azure disk or file share information.

### Step 7: Build and Push Docker Images

Build and push all Docker images to Docker Hub:

**First, log in to Docker Hub:**
```bash
docker login
# Enter your Docker Hub username and password (or access token)
```

**Then build and push images:**
```bash
export DOCKER_HUB_USERNAME="myname"  # Your Docker Hub username
./build-and-push-images.sh
```

**Note**: Adjust Dockerfile paths in the script based on your actual project structure.

**Important**: Make sure to build and push the `hbg-files-api` image:
```bash
# Example build command (adjust paths as needed)
docker build -t myname/hbg-files-api:latest -f src/Services/API/Files/Dockerfile .
docker push myname/hbg-files-api:latest
```

**For Private Repositories**: If your Docker Hub repositories are private, you'll need to create a Kubernetes secret for image pulling. See the "Docker Hub Private Repositories" section below.

**Note**: All deployment YAML files are pre-configured with `imagePullSecrets` pointing to `dockerhub-secret`. If you're using public repositories, you can remove the `imagePullSecrets` section from the deployment files (they're marked with comments).

### Step 8: Create Docker Hub Secret (If Using Private Repositories)

**Skip this step if using public repositories.**

If your Docker Hub repositories are private, create the Kubernetes secret before deploying:

```bash
export DOCKER_HUB_USERNAME="myname"
export DOCKER_HUB_PASSWORD="your-password-or-token"
export DOCKER_HUB_EMAIL="your-email@example.com"  # Optional
./setup-dockerhub-secret.sh
```

**Note**: All deployment YAML files are pre-configured with `imagePullSecrets`. If you're using public repositories, you can skip this step and remove the `imagePullSecrets` sections from deployment files.

### Step 9: Deploy Kubernetes Resources

Deploy all Kubernetes resources:

```bash
./deploy-k8s-resources.sh
```

This will deploy:
- Namespaces
- Secrets and ConfigMaps
- Database (PostgreSQL) with persistent storage
- Files API storage (PersistentVolumeClaim)
- Application services (STS, Admin, Admin SPA, SPA, Files API)
- Ingress configuration
- Monitoring (Uptime Kuma)

**Note**: The Files API storage will be automatically provisioned using Azure-managed disks. The deployment script waits for the PVC to be bound before deploying the Files API pod.

**Important**: If you're using private Docker Hub repositories and haven't created the secret yet, pods will fail to pull images. Create the secret (Step 8) before deploying, or create it after and restart pods: `kubectl rollout restart deployment -n default`

### Step 10: Verify Deployment

Check the status of all resources:

```bash
# Check pods
kubectl get pods --all-namespaces

# Check services
kubectl get services --all-namespaces

# Check ingress
kubectl get ingress --all-namespaces

# Check certificates
kubectl get certificates --all-namespaces

# Check certificate details
kubectl describe certificate hbg-cert -n default
```

Test services:

```bash
# Replace YOUR_DOMAIN with your actual domain
curl -k https://sts.YOUR_DOMAIN/health
curl -k https://spa.YOUR_DOMAIN/health
curl -k https://admin.YOUR_DOMAIN/health
curl -k https://filesapi.YOUR_DOMAIN/health
curl -k https://monitoring.YOUR_DOMAIN
```

Verify Files API storage:
```bash
# Check PVC status
kubectl get pvc hbg-files-storage-pvc

# Check Files API pod is using the storage
kubectl describe pod -l app=hbg-files-api | grep -A 5 "Mounts:"
```

## Docker Hub Private Repositories

If your Docker Hub repositories are private, you need to create a Kubernetes secret so AKS can pull images. **All deployment YAML files are already configured with `imagePullSecrets`**, so you just need to create the secret:

### Option 1: Use the Setup Script (Recommended)

```bash
export DOCKER_HUB_USERNAME="myname"
export DOCKER_HUB_PASSWORD="your-password-or-token"
export DOCKER_HUB_EMAIL="your-email@example.com"  # Optional
./setup-dockerhub-secret.sh
```

This script will:
- Create the `dockerhub-secret` in all required namespaces (default, monitoring)
- Handle namespace creation if needed
- Verify cluster connectivity

### Option 2: Manual Setup

```bash
# Create Docker Hub secret in default namespace
kubectl create secret docker-registry dockerhub-secret \
  --docker-server=https://index.docker.io/v1/ \
  --docker-username=myname \
  --docker-password=YOUR_DOCKER_HUB_PASSWORD \
  --docker-email=YOUR_EMAIL@example.com \
  --namespace=default

# Create in monitoring namespace (if using Uptime Kuma)
kubectl create secret docker-registry dockerhub-secret \
  --docker-server=https://index.docker.io/v1/ \
  --docker-username=myname \
  --docker-password=YOUR_DOCKER_HUB_PASSWORD \
  --docker-email=YOUR_EMAIL@example.com \
  --namespace=monitoring
```

**Using Docker Hub Access Tokens (Recommended for Security)**:
For better security, use an access token instead of your password:
1. Go to Docker Hub → Account Settings → Security → New Access Token
2. Create a token with read permissions
3. Use the token as `DOCKER_HUB_PASSWORD` in the script above

**Note**: All deployment YAML files already include `imagePullSecrets` configured to use `dockerhub-secret`. If you're using **public repositories**, you can remove the `imagePullSecrets` section from each deployment file (they're marked with comments).

**When to Create the Secret**:
- **Before deploying**: Create the secret before running `./deploy-k8s-resources.sh` if using private repos
- **After deploying**: If you forgot, create it and restart the pods: `kubectl rollout restart deployment -n default`

## Cost Optimization for Testing Environment

### Recommended Azure Resources:
- **Node VM Size**: `Standard_B2s` (2 vCPU, 4GB RAM) - ~$30/month per node
- **Node Count**: 2 nodes minimum for availability
- **Database Storage**: Standard SSD (LRS) - ~$0.10/GB/month (5GB = ~$0.50/month)
- **Files API Storage**: Standard SSD (LRS) - ~$0.10/GB/month (20GB = ~$2/month)
- **Docker Hub**: Free for public repos, or $7/month for unlimited private repos
- **Load Balancer**: Standard SKU - ~$25/month
- **Estimated Monthly Cost**: ~$95-150/month (saves ~$5/month vs ACR)

### Cost-Saving Tips:
1. Use `Standard_B2s` VM size (cheapest option with 2 vCPU)
2. Use Standard SSD instead of Premium SSD
3. Scale down to 1 node when not testing (not recommended for production)
4. Use Docker Hub free tier for public repositories
5. Delete resources when not in use

## Troubleshooting

### Pods in Pending State
```bash
kubectl describe pod <pod-name>
```
Common causes:
- PVC not bound - verify storage class
- Image pull errors - check ACR credentials

### Image Pull Errors
```bash
# Check if Docker Hub secret exists (for private repos)
kubectl get secret dockerhub-secret

# Verify Docker Hub login locally
docker login
docker pull myname/hbg-sts:latest

# Check pod events for image pull errors
kubectl describe pod <pod-name>

# Verify image name in deployment matches Docker Hub
kubectl get deployment <deployment-name> -o jsonpath='{.spec.template.spec.containers[0].image}'
```

### DNS Not Resolving
- Wait for DNS propagation (up to 60 minutes)
- Verify DNS records in Porkbun
- Test with: `dig sts.yourdomain.com`

### Certificate Not Issued

**For detailed TLS troubleshooting, see [TLS-SETUP-GUIDE.md](./TLS-SETUP-GUIDE.md#troubleshooting)**

Quick checks:
```bash
# Check cert-manager logs
kubectl logs -n cert-manager -l app=cert-manager

# Check certificate status
kubectl describe certificate hbg-cert -n default

# Check certificate requests
kubectl get certificaterequests

# Check challenges
kubectl get challenges
```

Common issues:
- Let's Encrypt rate limits (use staging issuer for testing)
- DNS not properly configured
- Ingress annotations incorrect
- DNS propagation not complete (wait 5-60 minutes)

### Services Not Accessible
- Verify ingress controller is running
- Check ingress rules: `kubectl describe ingress hbg-ingress`
- Verify DNS points to correct IP
- Check firewall rules in Azure

## Files Modified for Azure

The following files have been updated for Azure deployment:

1. **Storage**: 
   - `hbg-db-data.yaml` - Uses Azure-managed storage instead of hostPath
   - `hbg-files-storage.yaml` - New file for Files API persistent storage using Azure-managed disks
2. **Deployments**: All deployment files updated to pull from Docker Hub (`myname/hbg-*:latest`), including new `hbg-files-api.yaml`
3. **Domains**: All `.hbg.lol` references replaced with `YOUR_DOMAIN_NAME` placeholder
4. **Certificates**: Let's Encrypt issuer configured instead of self-signed
5. **Services**: Database service changed from NodePort to ClusterIP
6. **Files API**: New service added with persistent storage for image/file uploads
7. **Container Registry**: Using Docker Hub instead of Azure Container Registry (ACR)

## Manual Steps Required

Some steps require manual configuration:

1. **Docker Hub login**: Run `docker login` before building/pushing images
2. **Update Let's Encrypt email**: Edit `cert-manager/letsencrypt-issuer.yaml`
3. **Configure DNS**: Add A records in Porkbun pointing to ingress IP
4. **Review YAML files**: Verify all placeholders are replaced correctly
5. **Adjust Dockerfile paths**: Update `build-and-push-images.sh` if needed
6. **Docker Hub secret** (if using private repos): Run `./setup-dockerhub-secret.sh` or create manually

## Next Steps After Deployment

1. Configure Uptime Kuma monitors for all services (including Files API)
2. Set up backup strategy for PostgreSQL data
3. Set up backup strategy for Files API storage (consider Azure Backup or regular snapshots)
4. Configure monitoring and alerting
5. Review and harden security settings
6. Set up CI/CD pipeline for automated deployments
7. Configure log aggregation (Azure Monitor or similar)
8. Monitor Files API storage usage and adjust size as needed:
   ```bash
   kubectl get pvc hbg-files-storage-pvc
   # If storage is running low, edit hbg-files-storage.yaml and increase size
   ```

## Cleanup

To remove all resources:

```bash
# Delete Kubernetes resources
kubectl delete -f deploy-k8s-resources.sh

# Delete Docker Hub secrets (if created)
kubectl delete secret dockerhub-secret --all-namespaces

# Delete AKS cluster
az aks delete --resource-group <RESOURCE_GROUP> --name <AKS_CLUSTER_NAME>

# Delete resource group (deletes everything)
az group delete --name <RESOURCE_GROUP> --yes
```

**Note**: Docker Hub images remain in your Docker Hub account. To remove them:
1. Go to Docker Hub → Repositories
2. Delete repositories manually or use Docker Hub API

## Support

For issues:
- **Azure**: Check Azure documentation and support
- **Kubernetes**: Check pod logs and describe resources
- **cert-manager**: Check cert-manager logs and certificate status
- **DNS**: Verify DNS records and propagation



