# CI/CD Quick Start Guide

Your GitHub Actions CI/CD pipeline is ready to use! 🚀

## What Was Set Up

### ✅ Workflow Files Created

All workflows are in **two locations**:

1. **`/cicd/`** - Source files and documentation (this directory)
2. **`.github/workflows/`** - Active workflows (used by GitHub Actions)

### Active Workflows:

| Workflow | File | Trigger | Purpose |
|----------|------|---------|---------|
| **PR Validation** | `pr-validation.yml` | Pull requests | Validates code before merge |
| **Main Build** | `main-build.yml` | Push to main | Builds after merge |
| **Docker Build** | `docker-build.yml` | Manual/Tags | Builds Docker images |

### Template Files (Not Active):

| File | Purpose |
|------|---------|
| `deploy-k8s.yml.template` | Kubernetes deployment (rename to activate) |

## How to Use

### 1. PR Workflow (Most Common)

```bash
# Create feature branch
git checkout -b feature/my-awesome-feature

# Make changes, commit
git add .
git commit -m "Add awesome feature"

# Push to GitHub
git push -u origin feature/my-awesome-feature

# Create PR via GitHub web or mobile app
# → CI automatically runs (5-10 min)
# → Review checks on PR page
# → Merge when all checks pass ✅
```

### 2. Manual Docker Build

**Via GitHub Web:**
1. Go to: `https://github.com/exdrums/hbg/actions`
2. Click "Docker Build" workflow
3. Click "Run workflow"
4. Select service to build (all/files/constructor/spa)
5. Click "Run workflow" button

**Via GitHub Mobile:**
1. Open repository
2. Tap "Actions" tab
3. Tap "Docker Build"
4. Tap "Run workflow"
5. Select options
6. Tap "Run workflow"

### 3. Automatic Main Build

Runs automatically when you merge a PR to main branch.
No action needed! 🎉

## Next Steps

### Immediate (Do Now)

1. ✅ **Test the Pipeline**
   ```bash
   # Create a test PR
   git checkout -b test/ci-pipeline
   echo "# Test CI/CD" >> TEST.md
   git add TEST.md
   git commit -m "Test CI/CD pipeline"
   git push -u origin test/ci-pipeline
   # Create PR and watch checks run!
   ```

2. ✅ **Enable GitHub Actions** (if not already enabled)
   - Go to repo Settings → Actions → General
   - Ensure "Allow all actions" is selected

3. ✅ **Star Important Workflows**
   - Visit Actions tab to see workflow runs

### Optional (Later)

4. ⚙️ **Configure Docker Hub** (for Docker builds)
   ```
   Settings → Secrets and variables → Actions → New repository secret

   Add:
   - DOCKER_USERNAME: your-dockerhub-username
   - DOCKER_PASSWORD: your-dockerhub-token
   ```

5. ⚙️ **Enable Branch Protection** (recommended)
   ```
   Settings → Branches → Add branch protection rule

   Branch name pattern: main
   ☑️ Require status checks to pass before merging
   ☑️ Require branches to be up to date before merging

   Select required checks:
   - Build Backend
   - Test Backend
   - Build Frontend
   - Test Frontend
   ```

6. ⚙️ **Set Up Deployment** (when ready to deploy)
   ```bash
   # Rename template
   cd cicd
   cp deploy-k8s.yml.template ../github/workflows/deploy-k8s.yml

   # Add Kubernetes credentials to GitHub Secrets
   # Then run deployment workflow manually
   ```

## File Structure

```
/home/user/hbg/
├── .github/
│   └── workflows/              ← GitHub Actions reads from here
│       ├── pr-validation.yml   ← Active: PR checks
│       ├── main-build.yml      ← Active: Main branch build
│       └── docker-build.yml    ← Active: Docker images
│
└── cicd/                       ← Documentation and source files
    ├── README.md               ← Full documentation
    ├── QUICK-START.md          ← This file
    ├── MOBILE-WORKFLOW.md      ← Mobile PR guide
    ├── pr-validation.yml       ← Source file
    ├── main-build.yml          ← Source file
    ├── docker-build.yml        ← Source file
    └── deploy-k8s.yml.template ← Template (inactive)
```

## Understanding Workflow Runs

### On Pull Request:

```
You create PR
    ↓
GitHub Actions triggers "PR Validation"
    ↓
Runs in parallel:
├── Build Backend (3-5 min)
├── Build Frontend (2-4 min)
└── Lint TypeScript (0-1 min)
    ↓
Then runs:
├── Test Backend (1-2 min)
└── Test Frontend (1-2 min)
    ↓
All checks complete (5-10 min total)
    ↓
✅ Shows status on PR
    ↓
You can merge! 🎉
```

### On Merge to Main:

```
PR merged to main
    ↓
GitHub Actions triggers "Main Build"
    ↓
Full build + tests (5-10 min)
    ↓
Creates artifacts (optional)
    ↓
✅ Ready for deployment
```

## Common Tasks

### View Workflow Runs
```
GitHub Web: Repo → Actions tab
GitHub Mobile: Repo → Actions
```

### Cancel Running Workflow
```
GitHub Web: Actions → Click workflow run → Cancel workflow
GitHub Mobile: Actions → Workflow run → ⋮ → Cancel
```

### Re-run Failed Workflow
```
GitHub Web: Actions → Failed run → Re-run all jobs
GitHub Mobile: Actions → Failed run → ⋮ → Re-run
```

### Download Build Artifacts
```
GitHub Web: Actions → Workflow run → Artifacts section
(Main Build creates: api-files, api-constructor)
```

## Mobile App Setup

1. **Install GitHub Mobile**
   - iOS: https://apps.apple.com/app/github/id1477376905
   - Android: https://play.google.com/store/apps/details?id=com.github.android

2. **Enable Notifications**
   - App Settings → Notifications
   - Enable: Pull requests, Actions

3. **Bookmark Repository**
   - Open repo → Tap ⭐ to star

4. **Read Mobile Guide**
   - See `MOBILE-WORKFLOW.md` for detailed mobile instructions

## Troubleshooting

### Workflows Not Running?

**Check:**
1. Files are in `.github/workflows/` ✓
2. Files have `.yml` extension ✓
3. GitHub Actions enabled in Settings
4. No YAML syntax errors (check Actions tab)

### Checks Failing?

**Common issues:**
- Missing dependencies: Run `dotnet restore` or `npm install` locally first
- Test failures: Fix tests locally before pushing
- Linting errors: Run `npm run lint` locally and fix issues

### Slow Builds?

**Expected times:**
- Total PR validation: 8-12 minutes
- Main build: 10-15 minutes
- Docker build: 5-10 minutes per service

If much slower, check Actions tab for stuck jobs.

## Resources

- 📖 **Full Documentation**: See `README.md` in this directory
- 📱 **Mobile Guide**: See `MOBILE-WORKFLOW.md` in this directory
- 🔧 **GitHub Actions Docs**: https://docs.github.com/en/actions
- 🐳 **Docker Build Docs**: https://docs.docker.com/build/ci/github-actions/

## Support

Having issues? Check:
1. Actions tab for error messages
2. Workflow run logs
3. README.md for detailed explanations
4. GitHub Actions documentation

---

**Happy shipping! 🚀**

Last updated: 2025-11-18
