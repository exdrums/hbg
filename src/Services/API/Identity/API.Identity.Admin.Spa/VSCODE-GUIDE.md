# VS Code Setup and Debugging Guide - Identity Admin SPA

Complete guide for working with the Identity Admin SPA project in VS Code on macOS (M1 Pro).

## Table of Contents

1. [VS Code Workspace Setup](#vs-code-workspace-setup)
2. [Required Extensions](#required-extensions)
3. [Project Structure](#project-structure)
4. [Running Tests in VS Code](#running-tests-in-vs-code)
5. [Debugging Angular Application](#debugging-angular-application)
6. [Debugging ASP.NET Core](#debugging-aspnet-core)
7. [Docker Desktop Kubernetes Setup](#docker-desktop-kubernetes-setup)
8. [Full Stack Development Workflow](#full-stack-development-workflow)
9. [Troubleshooting](#troubleshooting)

---

## VS Code Workspace Setup

### Step 1: Open the Workspace

**Option A: Open specific project folder**
```bash
cd ~/path/to/hbg/src/Services/API/Identity/API.Identity.Admin.Spa
code .
```

**Option B: Open entire repository (recommended)**
```bash
cd ~/path/to/hbg
code .
```

### Step 2: Setup VS Code Configuration (First Time Only)

The repository includes pre-configured VS Code settings for this project.

**Copy the example configuration:**
```bash
# From the repository root
cp -r .vscode-example .vscode
```

This will set up:
- **launch.json** - Debug configurations for Angular and .NET
- **tasks.json** - Build tasks, test runners, and deployment scripts
- **settings.json** - Recommended editor settings
- **extensions.json** - List of recommended extensions

**Note:** The `.vscode` folder is gitignored so your personal settings won't be committed.

### Step 3: Verify Node.js and .NET Versions

Open integrated terminal in VS Code (`Ctrl + ~` or `View -> Terminal`):

```bash
# Check Node.js version (should be 18+)
node --version

# Check npm version
npm --version

# Check .NET version (should be 9.0+)
dotnet --version

# Check Docker version
docker --version

# Check kubectl version
kubectl version --client
```

If any tools are missing or wrong version, install them:

**Install Homebrew (if not installed):**
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

**Install required tools:**
```bash
# Node.js 18
brew install node@18
brew link node@18

# .NET 9.0 SDK
brew install --cask dotnet-sdk

# kubectl (if not already installed with Docker Desktop)
brew install kubectl
```

---

## Required Extensions

### Step 1: Open Extensions View

Press `Cmd + Shift + X` or click the Extensions icon in the sidebar.

### Step 2: Install Essential Extensions

**For Angular Development:**

1. **Angular Language Service** (by Angular)
   - Search: `Angular.ng-template`
   - Provides IntelliSense, error checking, navigation

2. **Angular Snippets** (by John Papa)
   - Search: `johnpapa.Angular2`
   - Code snippets for Angular

3. **TypeScript Importer** (by pmneo)
   - Search: `pmneo.tsimporter`
   - Auto-imports for TypeScript

**For .NET Development:**

4. **C# Dev Kit** (by Microsoft)
   - Search: `ms-dotnettools.csdevkit`
   - Includes C#, IntelliSense, debugging
   - Will also install C# extension automatically

5. **C#** (by Microsoft)
   - Search: `ms-dotnettools.csharp`
   - Usually installed with C# Dev Kit

**For Testing:**

6. **Jest Runner** (by firsttris)
   - Search: `firsttris.vscode-jest-runner`
   - Run individual Jest/Jasmine tests

7. **.NET Core Test Explorer** (by Jun Han)
   - Search: `formulahendry.dotnet-test-explorer`
   - Visual test runner for .NET tests

**For Docker/Kubernetes:**

8. **Docker** (by Microsoft)
   - Search: `ms-azuretools.vscode-docker`
   - Docker support, IntelliSense for Dockerfiles

9. **Kubernetes** (by Microsoft)
   - Search: `ms-kubernetes-tools.vscode-kubernetes-tools`
   - Kubernetes manifest IntelliSense, cluster management

**For General Development:**

10. **EditorConfig for VS Code** (by EditorConfig)
    - Search: `EditorConfig.EditorConfig`
    - Maintain consistent coding styles

11. **GitLens** (by GitKraken)
    - Search: `eamodio.gitlens`
    - Enhanced Git capabilities

12. **ESLint** (by Microsoft)
    - Search: `dbaeumer.vscode-eslint`
    - JavaScript/TypeScript linting

13. **Prettier** (by Prettier)
    - Search: `esbenp.prettier-vscode`
    - Code formatter

### Step 3: Configure Extensions

After installing, reload VS Code: `Cmd + Shift + P` → `Reload Window`

---

## Project Structure

Understanding the project layout in VS Code:

```
hbg/
├── src/
│   ├── Services/
│   │   └── API/
│   │       └── Identity/
│   │           ├── API.Identity.Admin.Spa/           # Main project
│   │           │   ├── Client/                       # Angular app
│   │           │   │   ├── src/
│   │           │   │   ├── package.json
│   │           │   │   ├── angular.json
│   │           │   │   └── karma.conf.js            # Test config
│   │           │   ├── Controllers/
│   │           │   ├── Program.cs
│   │           │   ├── Startup.cs
│   │           │   └── Dockerfile
│   │           └── API.Identity.Admin.Spa.Tests/    # .NET tests
│   │               ├── HealthCheckTests.cs
│   │               └── ConfigurationControllerTests.cs
│   └── Common/
│       └── Common/
│           └── AppSettings.cs
└── k8s/                                              # Kubernetes manifests
    ├── hbg-admin-spa.yaml
    ├── hbg-configmap.yaml
    └── hbg-ingress.yaml
```

---

## Running Tests in VS Code

### Angular Tests (Jasmine/Karma)

#### Method 1: Using Integrated Terminal

1. **Open terminal** (`Ctrl + ~`)

2. **Navigate to Client folder:**
   ```bash
   cd src/Services/API/Identity/API.Identity.Admin.Spa/Client
   ```

3. **Install dependencies (first time only):**
   ```bash
   npm install
   ```

4. **Run all tests:**
   ```bash
   npm test
   ```

   This will:
   - Start Karma test runner
   - Open Chrome browser
   - Run tests and watch for changes
   - Show results in terminal

5. **Run tests once (CI mode):**
   ```bash
   npm test -- --watch=false --browsers=ChromeHeadless
   ```

#### Method 2: Using NPM Scripts View

1. **Open NPM Scripts sidebar:**
   - Click "NPM SCRIPTS" in the Explorer view
   - Or: `Cmd + Shift + P` → `npm: Run Task`

2. **Find "test" script** under `client/package.json`

3. **Click the play icon** next to "test"

4. **View results** in the terminal panel

#### Method 3: Using Jest Runner Extension (for individual tests)

1. **Open a test file** (e.g., `clients.service.spec.ts`)

2. **You'll see "Run" and "Debug" links** above each `describe` and `it` block

3. **Click "Run"** to execute individual test

4. **Click "Debug"** to debug with breakpoints

#### Watch Test Coverage

```bash
npm run test:coverage
```

Then open `Client/coverage/index.html` in browser to view coverage report.

### .NET Tests (xUnit)

#### Method 1: Using Integrated Terminal

1. **Open terminal** (`Ctrl + ~`)

2. **Navigate to test project:**
   ```bash
   cd src/Services/API/Identity/API.Identity.Admin.Spa.Tests
   ```

3. **Run all tests:**
   ```bash
   dotnet test
   ```

4. **Run with detailed output:**
   ```bash
   dotnet test --verbosity detailed
   ```

5. **Run specific test:**
   ```bash
   dotnet test --filter "FullyQualifiedName~HealthCheckTests"
   ```

#### Method 2: Using Test Explorer

1. **Open Test Explorer:**
   - Click the beaker/flask icon in the sidebar
   - Or: `Cmd + Shift + P` → `Test: Focus on Test Explorer View`

2. **Discover tests:**
   - Click "Refresh Tests" icon
   - Tests will appear in tree view

3. **Run tests:**
   - Click "Run All Tests" icon (play button at top)
   - Or right-click individual test → "Run Test"

4. **Debug tests:**
   - Right-click test → "Debug Test"

5. **View results:**
   - Green checkmark = passed
   - Red X = failed
   - Click on test to see details

#### Method 3: Using Code Lens (inline)

1. **Open a test file** (e.g., `HealthCheckTests.cs`)

2. **You'll see "run test" and "debug test" links** above each `[Fact]`

3. **Click "run test"** to execute

4. **Click "debug test"** to debug with breakpoints

---

## Debugging Angular Application

### Step 1: Create Launch Configuration

1. **Create `.vscode` folder** in project root (if not exists):
   ```bash
   mkdir -p .vscode
   ```

2. **Create `launch.json`:**

Press `Cmd + Shift + P` → `Debug: Add Configuration` → Select "Chrome"

Or create manually:

`.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Angular: Chrome",
      "type": "chrome",
      "request": "launch",
      "url": "http://localhost:4201",
      "webRoot": "${workspaceFolder}/src/Services/API/Identity/API.Identity.Admin.Spa/Client",
      "sourceMaps": true,
      "sourceMapPathOverrides": {
        "webpack:/./*": "${webRoot}/*",
        "webpack:///src/*": "${webRoot}/src/*",
        "webpack:///./*": "${webRoot}/*",
        "webpack://*": "*"
      }
    },
    {
      "name": "Angular: Chrome (attach)",
      "type": "chrome",
      "request": "attach",
      "port": 9222,
      "url": "http://localhost:4201",
      "webRoot": "${workspaceFolder}/src/Services/API/Identity/API.Identity.Admin.Spa/Client"
    }
  ]
}
```

### Step 2: Start Angular Dev Server

1. **Open terminal** (`Ctrl + ~`)

2. **Navigate to Client folder:**
   ```bash
   cd src/Services/API/Identity/API.Identity.Admin.Spa/Client
   ```

3. **Start development server:**
   ```bash
   npm start
   ```

4. **Wait for message:**
   ```
   ✔ Compiled successfully.
   ** Angular Live Development Server is listening on localhost:4201 **
   ```

### Step 3: Start Debugging

1. **Set breakpoints:**
   - Open TypeScript file (e.g., `clients.service.ts`)
   - Click in the gutter (left of line numbers) to set red breakpoint

2. **Start debugger:**
   - Press `F5`
   - Or: Click "Run and Debug" in sidebar → Select "Angular: Chrome"

3. **Chrome will open** with the application

4. **Trigger your code:**
   - Navigate to the breakpoint location
   - Execution will pause at breakpoint

5. **Debug controls:**
   - `F5` - Continue
   - `F10` - Step Over
   - `F11` - Step Into
   - `Shift + F11` - Step Out
   - `Cmd + Shift + F5` - Restart
   - `Shift + F5` - Stop

6. **Inspect variables:**
   - Hover over variables to see values
   - Use "Variables" panel in Debug sidebar
   - Use "Watch" panel to monitor expressions

### Step 4: Debug Angular Tests

1. **Create test debug configuration** in `.vscode/launch.json`:

```json
{
  "name": "Angular: Test (Chrome)",
  "type": "chrome",
  "request": "launch",
  "url": "http://localhost:9876/debug.html",
  "webRoot": "${workspaceFolder}/src/Services/API/Identity/API.Identity.Admin.Spa/Client",
  "sourceMaps": true,
  "sourceMapPathOverrides": {
    "webpack:/./*": "${webRoot}/*",
    "webpack:///src/*": "${webRoot}/src/*"
  }
}
```

2. **Start tests in watch mode:**
   ```bash
   cd Client
   npm test
   ```

3. **Set breakpoints** in test files (`.spec.ts`)

4. **Select "Angular: Test (Chrome)"** from debug dropdown

5. **Press F5** to start debugging tests

---

## Debugging ASP.NET Core

### Step 1: Create Launch Configuration

The C# Dev Kit extension should auto-generate this. If not, create:

`.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET: Launch Admin SPA",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/Services/API/Identity/API.Identity.Admin.Spa/bin/Debug/net9.0/API.Identity.Admin.Spa.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/Services/API/Identity/API.Identity.Admin.Spa",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "sourceFileMap": {
        "/Views": "${workspaceFolder}/Views"
      }
    },
    {
      "name": ".NET: Attach to Process",
      "type": "coreclr",
      "request": "attach"
    }
  ]
}
```

### Step 2: Create Build Task

`.vscode/tasks.json`:
```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": [
        "build",
        "${workspaceFolder}/src/Services/API/Identity/API.Identity.Admin.Spa/API.Identity.Admin.Spa.csproj",
        "/property:GenerateFullPaths=true",
        "/consoleloggerparameters:NoSummary"
      ],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "watch",
      "command": "dotnet",
      "type": "process",
      "args": [
        "watch",
        "run",
        "--project",
        "${workspaceFolder}/src/Services/API/Identity/API.Identity.Admin.Spa/API.Identity.Admin.Spa.csproj"
      ],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

### Step 3: Start Debugging

1. **Set breakpoints:**
   - Open C# file (e.g., `ConfigurationController.cs`)
   - Click in gutter to set breakpoint

2. **Start debugger:**
   - Press `F5`
   - Or: Select ".NET: Launch Admin SPA" → Press `F5`

3. **Application will build and start**

4. **Browser opens automatically** when ready

5. **Trigger your code:**
   - Navigate to endpoint (e.g., `/configuration`)
   - Execution pauses at breakpoint

6. **Debug controls:**
   - Same as Angular (F5, F10, F11, etc.)

7. **Inspect variables:**
   - Use Debug Console to execute C# expressions
   - View call stack, variables, watch

### Step 4: Debug .NET Tests

1. **Open test file** (e.g., `HealthCheckTests.cs`)

2. **Set breakpoints** in test code

3. **Right-click on test** → "Debug Test"

4. **Or use Code Lens** "debug test" link above `[Fact]`

5. **Test will run in debug mode** and pause at breakpoints

---

## Docker Desktop Kubernetes Setup

### Step 1: Enable Kubernetes in Docker Desktop

1. **Open Docker Desktop** from Applications

2. **Click gear icon** (Settings)

3. **Go to "Kubernetes" tab**

4. **Check "Enable Kubernetes"**

5. **Click "Apply & Restart"**

6. **Wait for Kubernetes to start** (green indicator)

### Step 2: Verify Kubernetes Cluster

Open terminal in VS Code:

```bash
# Check cluster info
kubectl cluster-info

# Check nodes
kubectl get nodes

# Should see something like:
# NAME             STATUS   ROLES           AGE   VERSION
# docker-desktop   Ready    control-plane   10d   v1.28.2
```

### Step 3: Configure kubectl Context

```bash
# List contexts
kubectl config get-contexts

# Use docker-desktop context
kubectl config use-context docker-desktop

# Verify current context
kubectl config current-context
# Should output: docker-desktop
```

### Step 4: Create Kubernetes Resources

#### Apply ConfigMap

```bash
cd ~/path/to/hbg
kubectl apply -f k8s/hbg-configmap.yaml
```

#### Apply Secrets (if needed)

If you have a secrets file:
```bash
kubectl apply -f k8s/hbg-secret.yaml
```

#### Apply Database (if needed)

```bash
kubectl apply -f k8s/hbg-db.yaml
kubectl apply -f k8s/hbg-db-data.yaml
```

### Step 5: Build Docker Image for M1 Mac

**Important:** Build for ARM64 architecture:

```bash
cd ~/path/to/hbg

# Build for M1 (ARM64)
docker build \
  --platform linux/arm64 \
  -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
  -t exdrums/hbg-admin-spa:latest \
  .
```

**Note:** This may take 5-10 minutes on first build.

### Step 6: Verify Docker Image

```bash
# List images
docker images | grep hbg-admin-spa

# Test image locally
docker run -d \
  --platform linux/arm64 \
  --name hbg-admin-spa-test \
  -p 5796:80 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  exdrums/hbg-admin-spa:latest

# Wait 10 seconds then test
sleep 10
curl http://localhost:5796/health

# View logs
docker logs hbg-admin-spa-test

# Stop and remove
docker stop hbg-admin-spa-test
docker rm hbg-admin-spa-test
```

### Step 7: Deploy to Kubernetes

```bash
# Deploy application
kubectl apply -f k8s/hbg-admin-spa.yaml

# Check deployment
kubectl get deployments

# Check pods
kubectl get pods -l app=hbg-admin-spa

# Wait for pod to be ready
kubectl wait --for=condition=ready pod -l app=hbg-admin-spa --timeout=120s

# Check service
kubectl get service hbg-admin-spa-service
```

### Step 8: Setup Ingress

```bash
# Install NGINX Ingress Controller (if not installed)
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.1/deploy/static/provider/cloud/deploy.yaml

# Wait for ingress controller to be ready
kubectl wait --namespace ingress-nginx \
  --for=condition=ready pod \
  --selector=app.kubernetes.io/component=controller \
  --timeout=120s

# Apply ingress
kubectl apply -f k8s/hbg-ingress.yaml

# Check ingress
kubectl get ingress hbg-ingress
```

### Step 9: Configure /etc/hosts

```bash
# Add to /etc/hosts
echo "127.0.0.1 adminspa.hbg.local" | sudo tee -a /etc/hosts
echo "127.0.0.1 sts.hbg.local" | sudo tee -a /etc/hosts
echo "127.0.0.1 admin.hbg.local" | sudo tee -a /etc/hosts
echo "127.0.0.1 spa.hbg.local" | sudo tee -a /etc/hosts
```

### Step 10: Access Application

```bash
# If ingress doesn't have external IP, port forward:
kubectl port-forward --namespace=ingress-nginx service/ingress-nginx-controller 80:80 443:443
```

Then open in browser:
- **Application:** https://adminspa.hbg.local
- **Health Check:** https://adminspa.hbg.local/health
- **Health UI:** https://adminspa.hbg.local/hc

### Step 11: View Logs and Debug

```bash
# View pod logs
kubectl logs -l app=hbg-admin-spa -f

# Describe pod (for troubleshooting)
kubectl describe pod -l app=hbg-admin-spa

# Execute into pod
POD_NAME=$(kubectl get pods -l app=hbg-admin-spa -o jsonpath='{.items[0].metadata.name}')
kubectl exec -it $POD_NAME -- /bin/bash

# Inside pod, test health:
curl http://localhost:80/health
exit
```

---

## Full Stack Development Workflow

### Recommended VS Code Layout

1. **Split terminal** (click split icon in terminal):
   - Terminal 1: Angular dev server (`npm start`)
   - Terminal 2: ASP.NET Core (`dotnet watch run`)
   - Terminal 3: kubectl/docker commands

2. **Split editor:**
   - Left: Angular TypeScript files
   - Right: ASP.NET Core C# files

### Daily Development Flow

#### Morning Setup

1. **Start Docker Desktop**

2. **Open VS Code workspace:**
   ```bash
   cd ~/path/to/hbg
   code .
   ```

3. **Open 3 terminals:**

   **Terminal 1 - Angular:**
   ```bash
   cd src/Services/API/Identity/API.Identity.Admin.Spa/Client
   npm start
   ```

   **Terminal 2 - ASP.NET Core:**
   ```bash
   cd src/Services/API/Identity/API.Identity.Admin.Spa
   dotnet watch run
   ```

   **Terminal 3 - Commands:**
   ```bash
   # Available for kubectl, docker, git commands
   ```

4. **Access applications:**
   - Angular Dev: http://localhost:4201
   - ASP.NET Core: https://localhost:5796
   - Health UI: https://localhost:5796/hc

#### During Development

1. **Edit files** - both Angular and .NET will auto-reload

2. **Run tests frequently:**
   - Angular: Already running in watch mode
   - .NET: `dotnet test` in Terminal 3

3. **Debug as needed:**
   - Set breakpoints
   - Press F5 to debug
   - Use Debug Console for expressions

#### Before Committing

1. **Run all tests:**
   ```bash
   cd src/Services/API/Identity/API.Identity.Admin.Spa
   ./run-tests.sh all
   ```

2. **Build Docker image:**
   ```bash
   docker build --platform linux/arm64 \
     -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
     -t exdrums/hbg-admin-spa:latest .
   ```

3. **Test in Kubernetes:**
   ```bash
   kubectl delete -f k8s/hbg-admin-spa.yaml
   kubectl apply -f k8s/hbg-admin-spa.yaml
   kubectl logs -l app=hbg-admin-spa -f
   ```

---

## Troubleshooting

### Angular Issues

**Problem: "npm: command not found"**
```bash
# Install Node.js
brew install node@18
brew link node@18

# Verify
node --version
npm --version
```

**Problem: "Port 4201 already in use"**
```bash
# Find and kill process
lsof -ti:4201 | xargs kill -9

# Or change port in package.json:
# "start": "ng serve --port 4202"
```

**Problem: Tests not running**
```bash
# Clear cache and reinstall
cd Client
rm -rf node_modules package-lock.json
npm install
npm test
```

### .NET Issues

**Problem: "dotnet: command not found"**
```bash
# Install .NET SDK
brew install --cask dotnet-sdk

# Verify
dotnet --version
```

**Problem: "Could not find project"**
```bash
# Make sure you're in correct directory
cd src/Services/API/Identity/API.Identity.Admin.Spa

# Restore packages
dotnet restore

# Build
dotnet build
```

**Problem: Tests not discovered**
```bash
# Clean and rebuild
dotnet clean
dotnet build

# In VS Code: Cmd + Shift + P -> "Test: Refresh Tests"
```

### Docker/Kubernetes Issues

**Problem: "Cannot connect to Docker daemon"**
- Make sure Docker Desktop is running
- Check status bar in Docker Desktop (should be green)

**Problem: "Kubernetes is not available"**
- Open Docker Desktop Settings
- Go to Kubernetes tab
- Enable Kubernetes and wait for it to start

**Problem: "ImagePullBackOff" in Kubernetes**
```bash
# Make sure imagePullPolicy is set to Never
kubectl describe pod -l app=hbg-admin-spa

# Check if image exists locally
docker images | grep hbg-admin-spa

# Rebuild if needed
docker build --platform linux/arm64 \
  -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
  -t exdrums/hbg-admin-spa:latest .
```

**Problem: Pod keeps restarting**
```bash
# Check logs
kubectl logs -l app=hbg-admin-spa --previous

# Check events
kubectl get events --sort-by='.lastTimestamp'

# Describe pod
kubectl describe pod -l app=hbg-admin-spa
```

**Problem: "Cannot access https://adminspa.hbg.local"**
```bash
# Check /etc/hosts
cat /etc/hosts | grep hbg.local

# If missing, add it:
echo "127.0.0.1 adminspa.hbg.local" | sudo tee -a /etc/hosts

# Check ingress
kubectl get ingress

# Port forward if needed
kubectl port-forward --namespace=ingress-nginx service/ingress-nginx-controller 80:80 443:443
```

### VS Code Issues

**Problem: IntelliSense not working**
```bash
# For Angular:
cd Client
npm install

# For .NET:
cd ..
dotnet restore

# Reload VS Code window: Cmd + Shift + P -> "Reload Window"
```

**Problem: Debugger not attaching**
- Make sure dev server is running (npm start)
- Check port matches in launch.json (4201)
- Try Chrome in incognito mode
- Clear browser cache

**Problem: Extensions not working**
- Update extensions: Cmd + Shift + X -> Update All
- Reload window: Cmd + Shift + P -> "Reload Window"
- Reinstall extension if needed

---

## Quick Reference Commands

### Angular (from Client folder)

```bash
npm install              # Install dependencies
npm start               # Start dev server
npm test                # Run tests (watch mode)
npm run build           # Production build
npm run lint            # Run linter
```

### .NET (from project folder)

```bash
dotnet restore          # Restore packages
dotnet build            # Build project
dotnet run              # Run application
dotnet watch run        # Run with hot reload
dotnet test             # Run tests
dotnet clean            # Clean build output
```

### Docker

```bash
docker build --platform linux/arm64 -f Dockerfile -t exdrums/hbg-admin-spa:latest .
docker images                                    # List images
docker ps                                        # List running containers
docker logs <container-id>                       # View logs
docker exec -it <container-id> /bin/bash        # Shell into container
docker stop <container-id>                       # Stop container
docker rm <container-id>                         # Remove container
```

### Kubernetes

```bash
kubectl apply -f <file>                          # Apply manifest
kubectl get pods                                 # List pods
kubectl get services                             # List services
kubectl get deployments                          # List deployments
kubectl describe pod <pod-name>                  # Pod details
kubectl logs <pod-name> -f                       # Follow logs
kubectl delete -f <file>                         # Delete resources
kubectl port-forward service/<name> 8080:80     # Port forward
```

### Git

```bash
git status                                       # Check status
git add .                                        # Stage all changes
git commit -m "message"                          # Commit
git push                                         # Push to remote
git pull                                         # Pull from remote
```

---

## Support

If you encounter issues:

1. Check this guide's troubleshooting section
2. Check VS Code Output panel (View -> Output)
3. Check VS Code Problems panel (View -> Problems)
4. Check terminal error messages
5. Review TESTING.md and DEPLOYMENT.md for detailed guides

**Useful VS Code shortcuts:**
- `Cmd + Shift + P` - Command Palette
- `Cmd + P` - Quick Open file
- `Ctrl + ~` - Toggle terminal
- `F5` - Start debugging
- `Cmd + Shift + F` - Search in files
- `Cmd + Shift + E` - Explorer
- `Cmd + Shift + X` - Extensions
