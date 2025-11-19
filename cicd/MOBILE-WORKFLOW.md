# Mobile PR Workflow Guide

Quick guide for managing PRs from your phone using GitHub Mobile app.

## Prerequisites

1. Install **GitHub Mobile** app (iOS/Android)
2. Sign in to your GitHub account
3. Navigate to your `exdrums/hbg` repository

## Creating a PR from Your Phone

### Method 1: Using GitHub Mobile App (Recommended)

1. **Navigate to your repo** → Tap "Pull requests"
2. **Tap "+" button** in top right
3. **Select branches:**
   - Base: `main` (or target branch)
   - Compare: `your-feature-branch`
4. **Add title and description**
5. **Tap "Create pull request"**

### Method 2: Using Git on Phone (Termux/Working Copy)

```bash
# After committing your changes
git push origin your-feature-branch

# Then create PR via GitHub Mobile app or web browser
```

## Monitoring CI/CD Checks

### In GitHub Mobile App:

1. **Open your PR**
2. **Scroll to "Checks" section** (below description)
3. **View status:**
   - ⏳ Yellow dot = Running
   - ✅ Green check = Passed
   - ❌ Red X = Failed

### Check Details:

Tap on any check to see:
- Build logs
- Test results
- Error messages
- Execution time

## Common PR Scenarios

### Scenario 1: All Checks Pass ✅

```
✅ Build Backend        3m 24s
✅ Test Backend         1m 12s
✅ Build Frontend       2m 45s
✅ Test Frontend        1m 33s
✅ Lint TypeScript      0m 18s
```

**Action:** Tap "Merge pull request" → Confirm merge

### Scenario 2: One Check Fails ❌

```
✅ Build Backend        3m 24s
❌ Test Backend         Failed (1m 12s)
⏹️ Build Frontend       Skipped
⏹️ Test Frontend        Skipped
⏹️ Lint TypeScript      Skipped
```

**Action:**
1. Tap on failed check → View logs
2. Identify the error
3. Fix locally or ask team member
4. Push fix to same branch
5. Checks will re-run automatically

### Scenario 3: Checks Still Running ⏳

```
⏳ Build Backend        Running...
⏹️ Test Backend         Pending
⏹️ Build Frontend       Pending
⏹️ Test Frontend        Pending
⏹️ Lint TypeScript      Pending
```

**Action:** Wait 5-10 minutes for checks to complete. Pull down to refresh.

## Merging Strategies

### 1. Merge Commit (Default)
- **Use when:** You want to preserve full history
- **Command:** Tap "Merge pull request"
- **Result:** Creates merge commit

### 2. Squash and Merge
- **Use when:** Cleaning up commit history
- **Command:** Tap "▼" → Select "Squash and merge"
- **Result:** Combines all commits into one

### 3. Rebase and Merge
- **Use when:** You want linear history
- **Command:** Tap "▼" → Select "Rebase and merge"
- **Result:** Replays commits on top of base branch

## Reviewing Code on Mobile

### Quick Review Checklist:

1. **Read PR description** - What's being changed?
2. **Check "Files changed" tab** - Review the diff
3. **View CI check results** - All passing?
4. **Add comments** - Tap line numbers to comment
5. **Approve or Request changes**

### Adding Review Comments:

1. Navigate to **"Files changed"** tab
2. **Tap line number** where you want to comment
3. **Type your comment**
4. **Tap "Add single comment"** or "Start a review"

## Handling Failed Checks

### Common Failures and Quick Fixes:

#### Backend Build Failed 🔧
```
Error: Missing package reference
```
**Fix:** Add missing NuGet package in .csproj file

#### Backend Tests Failed 🧪
```
Error: 1 test(s) failed
```
**Fix:** Review test logs, fix failing test locally

#### Frontend Build Failed 📦
```
Error: Module not found
```
**Fix:** Run `npm install` locally, commit package-lock.json

#### Frontend Tests Failed 🎯
```
Error: Expected true, got false
```
**Fix:** Update test expectations or fix component logic

#### Lint Failed 📝
```
Error: Variable 'x' is never used
```
**Fix:** Remove unused code or add `// eslint-disable-line`

## Quick Commands (Via Mobile Terminal)

If using **Termux (Android)** or **Working Copy (iOS)**:

```bash
# Check current status
git status

# Create and push new branch
git checkout -b feature/my-feature
git add .
git commit -m "Add feature X"
git push -u origin feature/my-feature

# Update PR with fixes
git add .
git commit -m "Fix test failure"
git push

# Pull latest changes
git pull origin main

# View recent commits
git log --oneline -5
```

## Auto-Merge (Advanced)

### Enable Auto-Merge:

1. **Open PR** in GitHub Mobile
2. **Tap "..." menu** → "Enable auto-merge"
3. **Select merge strategy**
4. **Confirm**

Now PR will merge automatically when checks pass!

## Notifications Setup

### Get Notified of CI Status:

1. **GitHub Mobile** → Settings → Notifications
2. **Enable:**
   - Pull request reviews
   - Pull request comments
   - Actions workflows
   - Status checks

## Troubleshooting

### Checks Not Running?

**Causes:**
- Workflow file not in `.github/workflows/`
- YAML syntax error
- Insufficient permissions

**Fix:**
1. Check Actions tab for errors
2. Verify workflow file exists
3. Check repo Settings → Actions enabled

### Checks Taking Too Long?

**Normal times:**
- Build Backend: 3-5 min
- Test Backend: 1-2 min
- Build Frontend: 2-4 min
- Test Frontend: 1-2 min

**Total: ~8-10 minutes**

If longer than 15 min, check Actions tab for stuck jobs.

### Can't Merge?

**Common reasons:**
- ❌ Checks failed
- ⏳ Checks still running
- 🔒 Branch protection rules
- 🔄 Merge conflicts

**Fix:** Resolve the issue shown in PR status section

## Best Practices for Mobile PRs

1. ✅ **Keep PRs small** - Easier to review on mobile
2. ✅ **Write clear descriptions** - Help reviewers understand
3. ✅ **Wait for CI** - Don't merge with failing checks
4. ✅ **Test locally first** - Reduces CI failures
5. ✅ **Use draft PRs** - For work-in-progress
6. ✅ **Link issues** - Reference related issues with #123

## Emergency Rollback

If you merged something broken:

1. **Open merged PR**
2. **Tap "Revert"** button
3. **Create revert PR**
4. **Merge revert PR** (after CI passes)

## Useful Links

- GitHub Mobile: https://github.com/mobile
- GitHub Actions Status: https://www.githubstatus.com
- Repository Actions: `https://github.com/exdrums/hbg/actions`

---

**Pro Tip:** Save this repo to your GitHub Mobile favorites for quick access! ⭐
