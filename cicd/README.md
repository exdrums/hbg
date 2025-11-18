# CI/CD Pipeline Configuration

This directory contains GitHub Actions workflow configurations for the HBG microservices project.

## Overview

The CI/CD pipeline is designed to be **simple, fast, and mobile-friendly** for easy PR reviews and merges from your phone.

## Workflows

### 1. PR Validation (`pr-validation.yml`)
**Triggers:** Pull requests to any branch
**Purpose:** Validates code quality before merging

**What it does:**
- ✅ Builds .NET backend services
- ✅ Runs backend tests (xUnit)
- ✅ Builds Angular frontend
- ✅ Runs frontend tests (Jasmine/Karma)
- ✅ Lints TypeScript code
- ✅ Reports status back to PR (✓ or ✗)

**Duration:** ~5-8 minutes

### 2. Main Build (`main-build.yml`)
**Triggers:** Push to main branch
**Purpose:** Build and optionally deploy after merge

**What it does:**
- ✅ Full build of all services
- ✅ Runs all tests
- ✅ Builds Docker images (optional)
- ✅ Can trigger deployment workflows

### 3. Docker Build (`docker-build.yml`)
**Triggers:** Manual or on release tags
**Purpose:** Build and push Docker images to registry

**What it does:**
- 🐳 Builds Docker images for all services
- 🐳 Pushes to container registry
- 🐳 Tags with version numbers

## Setup Instructions

### Step 1: Copy Workflows to GitHub Actions Directory

GitHub Actions expects workflow files in `.github/workflows/`. Run:

```bash
# From project root
mkdir -p .github/workflows
cp cicd/*.yml .github/workflows/
```

### Step 2: Configure Secrets (if using Docker)

Add these secrets in GitHub Settings → Secrets and variables → Actions:

- `DOCKER_USERNAME` - Your Docker Hub username
- `DOCKER_PASSWORD` - Your Docker Hub password or access token

### Step 3: Enable GitHub Actions

1. Go to your repo on GitHub
2. Click "Actions" tab
3. Enable workflows if prompted

## Mobile-Friendly PR Workflow

When creating PRs from your phone:

1. **Push your branch** to GitHub
2. **Create PR** via GitHub mobile app or web
3. **Wait for checks** - Status will show in PR (usually 5-8 min)
4. **Review results** - Green ✓ = good to merge, Red ✗ = fix needed
5. **Merge** - Tap "Merge pull request" when all checks pass

## Workflow Files

| File | Purpose | Trigger |
|------|---------|---------|
| `pr-validation.yml` | Validate PRs before merge | On PR open/update |
| `main-build.yml` | Build after merge | Push to main |
| `docker-build.yml` | Build Docker images | Manual/Tags |

## Understanding the Status Checks

When you view a PR on mobile, you'll see:

```
✓ Build Backend       Passed in 3m 24s
✓ Test Backend        Passed in 1m 12s
✓ Build Frontend      Passed in 2m 45s
✓ Test Frontend       Passed in 1m 33s
✓ Lint TypeScript     Passed in 0m 18s
```

- **Green checkmark (✓):** All good! Safe to merge
- **Red X (✗):** Something failed, needs fixing
- **Yellow dot (●):** Still running, wait a bit

## Customization

### Run Only Specific Tests

Edit workflow files to comment out jobs you don't need:

```yaml
jobs:
  build-backend:
    # ... keep this

  # test-frontend:  # Comment out to skip
  #   # ... frontend tests
```

### Change Branch Protection

In GitHub Settings → Branches → Add rule:
- Branch name pattern: `main`
- ✓ Require status checks to pass before merging
- Select which checks are required

## Troubleshooting

### Workflow not running?
- Check `.github/workflows/` exists
- Verify workflow files have `.yml` extension
- Check Actions tab for error messages

### Tests failing in CI but pass locally?
- Ensure `.gitignore` isn't excluding test files
- Check for environment-specific config
- Review workflow logs in Actions tab

### Docker build failing?
- Verify Dockerfile paths are correct
- Check Docker Hub credentials
- Ensure enough disk space in runner

## Quick Commands

```bash
# Test backend locally (like CI does)
dotnet build
dotnet test

# Test frontend locally (like CI does)
cd src/Web/Web.SPA/Client
npm install
npm run build
npm run test:ci

# Build Docker image locally
docker build -t test-image -f src/Services/API/Files/Dockerfile .
```

## Next Steps

1. **Start simple:** Use only `pr-validation.yml` at first
2. **Add Docker builds:** When ready to deploy
3. **Add deployments:** Set up K8s deployment workflow
4. **Add notifications:** Slack/email on build failures

## Resources

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [.NET CI/CD Guide](https://docs.microsoft.com/en-us/dotnet/devops/)
- [Angular CI Guide](https://angular.io/guide/deployment)

---

**Created:** 2025-11-18
**Project:** HBG Microservices
**Maintainer:** @exdrums
