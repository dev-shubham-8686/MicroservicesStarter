# CI/CD Quick Start Guide

Get your CI/CD pipeline up and running in 5 minutes!

## 🚀 Quick Setup

### Step 1: Enable GitHub Actions

1. Go to your GitHub repository
2. Click **Settings** → **Actions** → **General**
3. Under **Workflow permissions**, select:
   - ✅ **Read and write permissions**
   - ✅ **Allow GitHub Actions to create and approve pull requests**
4. Click **Save**

### Step 2: Push Your Code

The CI pipeline will automatically run when you push to `main` or `develop`:

```bash
git add .
git commit -m "Add CI/CD pipeline"
git push origin main
```

### Step 3: Watch It Run

1. Go to the **Actions** tab in your GitHub repository
2. You'll see your workflow running
3. Click on it to see detailed logs

## 📋 What Happens Automatically

### On Every Push (CI Pipeline)

✅ **Build** - Compiles your .NET solution  
✅ **Test** - Runs all tests  
✅ **Lint** - Checks code quality  
✅ **Docker Build** - Builds all Docker images  
✅ **Security Scan** - Scans for vulnerabilities  

### On Push to Main (CD Pipeline)

✅ **Version** - Calculates version number  
✅ **Build & Push** - Builds and pushes Docker images to registry  
✅ **Deploy Staging** - Deploys to staging environment  
✅ **Smoke Tests** - Tests deployed services  
✅ **Create Release** - Creates GitHub release (if tagged)  

## 🎯 Common Tasks

### View Pipeline Status

```bash
# Via GitHub UI
# Go to: Actions tab → Select workflow run

# Via GitHub CLI
gh run list
gh run view --web
```

### Manually Trigger Deployment

1. Go to **Actions** → **Manual Deployment**
2. Click **Run workflow**
3. Select:
   - Environment: `staging` or `production`
   - Version: `latest` or specific tag
   - Services: Leave empty for all, or specify comma-separated list
4. Click **Run workflow**

### Deploy Locally

```bash
# Linux/Mac
./scripts/deploy.sh staging latest

# Windows PowerShell
.\scripts\deploy.ps1 staging latest
```

### View Docker Images

Images are published to GitHub Container Registry:

```
ghcr.io/your-username/your-repo/identityservice:latest
ghcr.io/your-username/your-repo/productservice:latest
ghcr.io/your-username/your-repo/orderservice:latest
ghcr.io/your-username/your-repo/apigateway:latest
```

## 🔧 Configuration

### Environment Variables

Create `.env.production` file (copy from `.env.production.example`):

```bash
cp .env.production.example .env.production
# Edit .env.production with your values
```

### GitHub Secrets (Optional)

If deploying to external services, add secrets:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Add secrets like:
   - `DOCKER_REGISTRY_TOKEN`
   - `KUBERNETES_SECRET`
   - `AZURE_CREDENTIALS`

### Environments

Set up protected environments:

1. Go to **Settings** → **Environments**
2. Create `staging` and `production` environments
3. Configure:
   - Required reviewers (for production)
   - Deployment branches
   - Environment variables

## 📊 Pipeline Status Badge

Add to your README.md:

```markdown
![CI](https://github.com/your-username/your-repo/workflows/CI/badge.svg)
![CD](https://github.com/your-username/your-repo/workflows/CD/badge.svg)
```

## 🐛 Troubleshooting

### Pipeline Fails on Build

**Check:**
- .NET SDK version matches (8.0.x)
- All dependencies are restored
- No compilation errors

**Fix:**
```bash
# Test locally first
dotnet restore
dotnet build
```

### Docker Build Fails

**Check:**
- Dockerfile syntax is correct
- All referenced files exist
- Base images are available

**Fix:**
```bash
# Test Docker build locally
docker build -f src/services/IdentityService/Dockerfile -t test .
```

### Tests Fail

**Check:**
- Tests pass locally
- Test data is available
- Test configuration is correct

**Fix:**
```bash
# Run tests locally
dotnet test
```

### Deployment Fails

**Check:**
- Environment variables are set
- Secrets are configured
- Network connectivity
- Permissions

**Fix:**
- Check deployment logs in Actions tab
- Verify environment configuration
- Test deployment manually

## 📚 Next Steps

1. ✅ **Read Full Guide** - [CICD_GUIDE.md](CICD_GUIDE.md) for detailed explanations
2. ✅ **Add Tests** - Create test projects for better CI coverage
3. ✅ **Configure Environments** - Set up staging and production
4. ✅ **Set Up Monitoring** - Monitor deployments and services
5. ✅ **Create Releases** - Tag versions for releases

## 🎓 Learning Resources

- **GitHub Actions Docs:** https://docs.github.com/en/actions
- **Docker Best Practices:** https://docs.docker.com/develop/dev-best-practices/
- **.NET CI/CD:** https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test

## 💡 Tips

1. **Start Small** - Get basic CI working first, then add CD
2. **Test Locally** - Always test changes locally before pushing
3. **Monitor Logs** - Check pipeline logs regularly
4. **Use Caching** - Enable build caching for faster pipelines
5. **Fail Fast** - Fix issues immediately, don't let them accumulate

---

**Happy CI/CDing!** 🚀

For detailed explanations, see [CICD_GUIDE.md](CICD_GUIDE.md)

