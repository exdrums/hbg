# Quick Start Guide - MacBook Pro M1

Get up and running with the Identity Admin SPA in 10 minutes.

## Prerequisites Check

Open Terminal and run:

```bash
# Check versions
node --version    # Should be 18+
npm --version     # Should be 9+
dotnet --version  # Should be 9.0+
docker --version  # Should be installed
kubectl version --client  # Should be installed

# If any are missing, install them:
# Homebrew (package manager)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Node.js 18
brew install node@18

# .NET 9.0
brew install --cask dotnet-sdk

# Docker Desktop (includes kubectl)
# Download from: https://www.docker.com/products/docker-desktop/
```

## Step 1: Clone and Open in VS Code

```bash
# Navigate to your projects folder
cd ~/Projects  # or wherever you keep code

# Clone repository (if not already cloned)
# git clone <your-repo-url> hbg

# Open in VS Code
cd hbg
code .
```

## Step 2: Install VS Code Extensions

When VS Code opens, you'll see a prompt to install recommended extensions.

**Click "Install All"** or install manually:

1. Press `Cmd + Shift + X` (Extensions)
2. Search and install:
   - Angular Language Service
   - C# Dev Kit
   - Docker
   - Kubernetes

## Step 3: Install Dependencies

Open VS Code Terminal (`Ctrl + ~`):

```bash
# Install Angular dependencies
cd src/Services/API/Identity/API.Identity.Admin.Spa/Client
npm install

# Restore .NET dependencies
cd ..
dotnet restore
```

## Step 4: Run Tests

### Angular Tests

```bash
cd Client
npm test
```

Press `Ctrl + C` to stop tests.

### .NET Tests

```bash
cd ../API.Identity.Admin.Spa.Tests
dotnet test
```

## Step 5: Run Development Servers

Open **3 terminals** in VS Code (click split terminal icon):

**Terminal 1 - Angular:**
```bash
cd src/Services/API/Identity/API.Identity.Admin.Spa/Client
npm start
```

Wait for:
```
✔ Compiled successfully.
** Angular Live Development Server is listening on localhost:4201 **
```

**Terminal 2 - ASP.NET Core:**
```bash
cd src/Services/API/Identity/API.Identity.Admin.Spa
dotnet watch run
```

Wait for:
```
Now listening on: https://localhost:5796
```

**Terminal 3 - Available for commands**

## Step 6: Open Application

Open browser:
- **Angular Dev**: http://localhost:4201
- **ASP.NET Core**: https://localhost:5796
- **Health Checks UI**: https://localhost:5796/hc
- **Configuration API**: https://localhost:5796/configuration

## Step 7: Debug in VS Code

### Debug Angular

1. Open Angular file (e.g., `clients.service.ts`)
2. Click in the gutter to set breakpoint (red dot)
3. Press `F5`
4. Select "Angular: Chrome"
5. Navigate to the code in browser - execution will pause

### Debug .NET

1. Open C# file (e.g., `ConfigurationController.cs`)
2. Set breakpoint
3. Press `F5`
4. Select ".NET: Launch Admin SPA"
5. Make request to endpoint - execution will pause

## Step 8: Deploy to Kubernetes (Optional)

### Enable Kubernetes in Docker Desktop

1. Open **Docker Desktop**
2. Click gear icon (Settings)
3. Go to **Kubernetes** tab
4. Check **"Enable Kubernetes"**
5. Click **"Apply & Restart"**
6. Wait for green indicator

### Build Docker Image

```bash
cd ~/Projects/hbg  # or your repo location

docker build \
  --platform linux/arm64 \
  -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
  -t exdrums/hbg-admin-spa:latest \
  .
```

**This will take 5-10 minutes on first build.**

### Deploy to Kubernetes

```bash
# Apply configuration
kubectl apply -f k8s/hbg-configmap.yaml

# Deploy application
kubectl apply -f k8s/hbg-admin-spa.yaml

# Check status
kubectl get pods -l app=hbg-admin-spa

# View logs
kubectl logs -l app=hbg-admin-spa -f
```

### Setup Ingress

```bash
# Install NGINX Ingress Controller
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# Wait for it to be ready
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=120s

# Apply ingress
kubectl apply -f k8s/hbg-ingress.yaml

# Add to /etc/hosts
echo "127.0.0.1 adminspa.hbg.lol" | sudo tee -a /etc/hosts

# Port forward
kubectl port-forward --namespace=ingress-nginx service/ingress-nginx-controller 80:80 443:443
```

### Access Application

Open browser: **https://adminspa.hbg.lol**

## Common Commands

### Development

```bash
# Run Angular dev server
npm start

# Run ASP.NET Core with hot reload
dotnet watch run

# Run all tests
./run-tests.sh all
```

### Docker

```bash
# Build for M1 Mac
docker build --platform linux/arm64 -f Dockerfile -t exdrums/hbg-admin-spa:latest .

# Run locally
docker run -d -p 5796:80 --name test exdrums/hbg-admin-spa:latest

# View logs
docker logs test -f

# Stop and remove
docker stop test && docker rm test
```

### Kubernetes

```bash
# Deploy
kubectl apply -f k8s/hbg-admin-spa.yaml

# Check status
kubectl get pods -l app=hbg-admin-spa

# View logs
kubectl logs -l app=hbg-admin-spa -f

# Delete
kubectl delete -f k8s/hbg-admin-spa.yaml
```

## VS Code Shortcuts

- `Cmd + P` - Quick open file
- `Cmd + Shift + P` - Command palette
- `Ctrl + ~` - Toggle terminal
- `F5` - Start debugging
- `Cmd + Shift + F` - Search in files
- `Cmd + /` - Toggle comment
- `Option + Shift + F` - Format document

## Troubleshooting

### Port 4201 already in use
```bash
lsof -ti:4201 | xargs kill -9
```

### Port 5796 already in use
```bash
lsof -ti:5796 | xargs kill -9
```

### npm install fails
```bash
cd Client
rm -rf node_modules package-lock.json
npm install
```

### Docker build fails
```bash
# Make sure Docker Desktop is running
# Check available space: df -h
# Try building again with --no-cache
docker build --no-cache --platform linux/arm64 -f Dockerfile -t exdrums/hbg-admin-spa:latest .
```

### Kubernetes pod not starting
```bash
# Check pod status
kubectl describe pod -l app=hbg-admin-spa

# Check logs
kubectl logs -l app=hbg-admin-spa

# Check events
kubectl get events --sort-by='.lastTimestamp'
```

## Next Steps

- Read **VSCODE-GUIDE.md** for detailed VS Code setup
- Read **TESTING.md** for comprehensive testing guide
- Read **DEPLOYMENT.md** for production deployment
- Read **README.md** for project overview

## Getting Help

1. Check VS Code Output panel: `Cmd + Shift + U`
2. Check VS Code Problems panel: `Cmd + Shift + M`
3. Check terminal error messages
4. Review the detailed guides mentioned above

---

**You're ready to go!** 🚀

Start both servers (Angular + .NET), set some breakpoints, and press F5 to debug.
