# Complete CI/CD Pipeline Guide
## From Beginner to Advanced

This comprehensive guide explains how to set up, understand, and use the CI/CD pipeline for this microservices project.

---

## Table of Contents

1. [What is CI/CD?](#what-is-cicd)
2. [Pipeline Overview](#pipeline-overview)
3. [GitHub Actions Workflows Explained](#github-actions-workflows-explained)
4. [Setting Up CI/CD](#setting-up-cicd)
5. [Understanding Each Workflow](#understanding-each-workflow)
6. [Docker Build Optimization](#docker-build-optimization)
7. [Deployment Strategies](#deployment-strategies)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

---

## What is CI/CD?

### CI (Continuous Integration)

**Continuous Integration** means:
- ✅ Code is automatically built and tested when pushed
- ✅ Issues are caught early before they reach production
- ✅ Multiple developers can work together safely
- ✅ Code quality is maintained automatically

**What happens in CI:**
1. Developer pushes code to repository
2. CI pipeline automatically:
   - Builds the code
   - Runs tests
   - Checks code quality
   - Builds Docker images
   - Scans for security issues

### CD (Continuous Deployment/Delivery)

**Continuous Deployment** means:
- ✅ Code is automatically deployed after passing CI
- ✅ Manual approval gates for production
- ✅ Consistent deployment process
- ✅ Quick rollback capabilities

**What happens in CD:**
1. After CI passes, CD pipeline:
   - Builds production Docker images
   - Pushes to container registry
   - Deploys to staging environment
   - Runs smoke tests
   - Deploys to production (with approval)

---

## Pipeline Overview

Our CI/CD pipeline consists of multiple GitHub Actions workflows:

```
┌─────────────────────────────────────────────────────────┐
│                    Developer Push                       │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │   CI Pipeline (ci.yml) │
        │  • Build                │
        │  • Test                │
        │  • Lint                │
        │  • Docker Build        │
        │  • Security Scan       │
        └────────────┬───────────┘
                     │
                     ▼ (if passes)
        ┌────────────────────────┐
        │   CD Pipeline (cd.yml) │
        │  • Version              │
        │  • Build & Push Images │
        │  • Deploy Staging      │
        │  • Smoke Tests         │
        │  • Create Release      │
        └────────────────────────┘
```

---

## GitHub Actions Workflows Explained

### 1. CI Pipeline (`.github/workflows/ci.yml`)

**Purpose:** Run on every push and pull request to ensure code quality.

**Jobs:**

#### Job 1: Build Solution
```yaml
build:
  name: Build Solution
  runs-on: ubuntu-latest
```

**What it does:**
- Checks out code from repository
- Sets up .NET 8 SDK
- Restores NuGet packages
- Builds the entire solution
- Uploads build artifacts

**Why it's important:**
- Catches compilation errors immediately
- Ensures all projects build successfully
- Creates artifacts for other jobs

#### Job 2: Run Tests
```yaml
test:
  name: Run Tests
  needs: build
```

**What it does:**
- Runs all unit tests
- Generates test reports
- Uploads test results

**Why it's important:**
- Ensures code changes don't break existing functionality
- Provides test coverage metrics

#### Job 3: Code Quality (Lint)
```yaml
lint:
  name: Code Quality
```

**What it does:**
- Checks code formatting
- Runs code analysis
- Validates coding standards

**Why it's important:**
- Maintains consistent code style
- Catches potential issues early

#### Job 4: Build Docker Images
```yaml
docker-build:
  name: Build Docker Images
  strategy:
    matrix:
      service: [identityservice, productservice, orderservice, apigateway]
```

**What it does:**
- Builds Docker images for each service
- Uses Docker Buildx for advanced features
- Caches layers for faster builds
- Tests that images build successfully

**Why it's important:**
- Ensures Docker images can be built
- Validates Dockerfile correctness
- Prepares for deployment

#### Job 5: Security Scan
```yaml
security:
  name: Security Scan
```

**What it does:**
- Scans code for vulnerabilities
- Checks dependencies for security issues
- Uploads results to GitHub Security

**Why it's important:**
- Identifies security vulnerabilities early
- Protects against known CVEs
- Maintains security posture

### 2. CD Pipeline (`.github/workflows/cd.yml`)

**Purpose:** Deploy code to environments after CI passes.

**Jobs:**

#### Job 1: Determine Version
```yaml
version:
  name: Determine Version
  outputs:
    version: ${{ steps.version.outputs.version }}
    tag: ${{ steps.version.outputs.tag }}
```

**What it does:**
- Calculates version from git tag or commit SHA
- Creates appropriate Docker tags
- Sets version for deployment

**Versioning Strategy:**
- **Tagged releases:** `v1.0.0` → version `1.0.0`
- **Branch commits:** `20241215-abc12345` (date + SHA)

#### Job 2: Build and Push Docker Images
```yaml
build-and-push:
  strategy:
    matrix:
      service: [identityservice, productservice, orderservice, apigateway]
```

**What it does:**
- Builds production Docker images
- Tags images with version
- Pushes to GitHub Container Registry (ghcr.io)
- Uses build cache for speed

**Image Tags Created:**
- `latest` - Latest from main branch
- `v1.0.0` - Specific version
- `main-abc12345` - Branch + commit SHA

#### Job 3: Update Docker Compose
```yaml
update-compose:
  name: Update Docker Compose
```

**What it does:**
- Creates production docker-compose configuration
- Updates image tags
- Prepares for deployment

#### Job 4: Deploy to Staging
```yaml
deploy-staging:
  name: Deploy to Staging
  environment:
    name: staging
```

**What it does:**
- Deploys to staging environment
- Uses environment-specific configuration
- Waits for services to be ready

**Environment Protection:**
- Can require approvals
- Can restrict who can deploy
- Can set environment variables

#### Job 5: Smoke Tests
```yaml
smoke-tests:
  name: Smoke Tests
  needs: [deploy-staging]
```

**What it does:**
- Tests critical endpoints
- Verifies services are running
- Validates deployment success

**Why it's important:**
- Catches deployment issues early
- Ensures services are functional
- Prevents broken deployments

#### Job 6: Create GitHub Release
```yaml
create-release:
  name: Create Release
  if: startsWith(github.ref, 'refs/tags/v')
```

**What it does:**
- Creates GitHub release for version tags
- Documents what's in the release
- Provides download links

### 3. Docker Build (`.github/workflows/docker-build.yml`)

**Purpose:** Build only changed services (optimization).

**What it does:**
- Detects which services changed
- Builds only changed services
- Saves CI/CD time and resources

**Use Case:**
- When only one service changes
- Faster feedback loop
- Reduced build time

### 4. Manual Deployment (`.github/workflows/manual-deploy.yml`)

**Purpose:** Manual deployment to any environment.

**What it does:**
- Allows manual trigger
- Selects environment (staging/production)
- Chooses version to deploy
- Deploys specific services or all

**Use Case:**
- Emergency deployments
- Rollback to previous version
- Deploy to specific environment

---

## Setting Up CI/CD

### Step 1: Enable GitHub Actions

1. Go to your GitHub repository
2. Click **Settings** → **Actions** → **General**
3. Enable **Actions** and **Workflow permissions**
4. Allow **Read and write permissions**

### Step 2: Set Up Container Registry

1. Go to **Settings** → **Packages**
2. GitHub Container Registry (ghcr.io) is automatically available
3. Images will be published to: `ghcr.io/your-username/your-repo/service-name`

### Step 3: Configure Secrets (if needed)

For external deployments, you may need secrets:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Add secrets:
   - `DOCKER_REGISTRY_TOKEN` (if using external registry)
   - `KUBERNETES_SECRET` (if deploying to Kubernetes)
   - `AZURE_CREDENTIALS` (if deploying to Azure)

### Step 4: Set Up Environments

1. Go to **Settings** → **Environments**
2. Create environments:
   - **staging** - For staging deployments
   - **production** - For production deployments
3. Configure protection rules:
   - Required reviewers
   - Deployment branches
   - Wait timer

### Step 5: Push Code and Watch

1. Push code to `main` or `develop` branch
2. Go to **Actions** tab in GitHub
3. Watch the pipeline run in real-time
4. Check results and logs

---

## Understanding Each Workflow

### CI Workflow Deep Dive

#### Triggers
```yaml
on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]
```

**When it runs:**
- Every push to `main` or `develop`
- Every pull request to `main` or `develop`

#### Job Dependencies
```yaml
test:
  needs: build  # Runs after build completes
```

**Dependency Chain:**
1. `build` runs first
2. `test` runs after `build` (needs artifacts)
3. `docker-build` runs after `build` and `test`
4. All can run in parallel where possible

#### Matrix Strategy
```yaml
strategy:
  matrix:
    service:
      - name: identityservice
        dockerfile: src/services/IdentityService/Dockerfile
```

**What it does:**
- Runs the same job for multiple services
- Builds all services in parallel
- Reduces total pipeline time

### CD Workflow Deep Dive

#### Version Calculation
```yaml
if [[ "${{ github.ref }}" == refs/tags/* ]]; then
  VERSION=${GITHUB_REF#refs/tags/v}
else
  VERSION=$(date +'%Y%m%d')-${GITHUB_SHA::8}
fi
```

**Version Examples:**
- Tag `v1.0.0` → version `1.0.0`
- Commit on main → `20241215-abc12345`

#### Docker Build and Push
```yaml
uses: docker/build-push-action@v5
with:
  context: .
  file: ${{ matrix.service.dockerfile }}
  push: true
  tags: ${{ steps.meta.outputs.tags }}
  cache-from: type=gha
  cache-to: type=gha,mode=max
```

**Key Features:**
- **Context:** Root directory (for shared code)
- **File:** Specific Dockerfile for service
- **Push:** Uploads to registry
- **Cache:** Uses GitHub Actions cache for speed

#### Environment Protection
```yaml
environment:
  name: staging
  url: https://staging.example.com
```

**Protection Options:**
- Required reviewers
- Wait timer before deployment
- Deployment branches restriction
- Environment-specific secrets

---

## Docker Build Optimization

### Multi-Stage Builds

Our Dockerfiles already use multi-stage builds:

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... build steps ...

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
COPY --from=build /app/publish .
```

**Benefits:**
- Smaller final images
- Faster deployments
- Better security

### Build Cache

```yaml
cache-from: type=gha
cache-to: type=gha,mode=max
```

**What it does:**
- Caches Docker layers in GitHub Actions
- Reuses layers between builds
- Speeds up builds significantly

**Cache Strategy:**
- Cache dependencies layer (changes rarely)
- Cache build layer (changes when code changes)
- Don't cache final layer (always fresh)

### Build Arguments

```yaml
build-args: |
  BUILD_DATE=${{ github.event.head_commit.timestamp }}
  VCS_REF=${{ github.sha }}
  VERSION=${{ needs.version.outputs.version }}
```

**Use in Dockerfile:**
```dockerfile
ARG BUILD_DATE
ARG VCS_REF
ARG VERSION

LABEL org.opencontainers.image.created=$BUILD_DATE
LABEL org.opencontainers.image.revision=$VCS_REF
LABEL org.opencontainers.image.version=$VERSION
```

---

## Deployment Strategies

### 1. Blue-Green Deployment

**How it works:**
- Deploy new version alongside old version
- Switch traffic to new version
- Keep old version for quick rollback

**Benefits:**
- Zero downtime
- Quick rollback
- Easy testing

### 2. Rolling Deployment

**How it works:**
- Deploy new version gradually
- Replace instances one by one
- Maintain service availability

**Benefits:**
- Gradual rollout
- Lower risk
- Can pause if issues found

### 3. Canary Deployment

**How it works:**
- Deploy new version to small percentage
- Monitor for issues
- Gradually increase percentage

**Benefits:**
- Early issue detection
- Low risk
- Real-world testing

### Implementation Example

```yaml
deploy-staging:
  steps:
    - name: Deploy new version
      run: |
        # Deploy to 10% of instances (canary)
        kubectl set image deployment/service service=image:v2.0.0 --replicas=1
        
        # Wait and monitor
        sleep 60
        
        # If healthy, deploy to 100%
        kubectl set image deployment/service service=image:v2.0.0 --replicas=10
```

---

## Best Practices

### 1. Keep Pipelines Fast

✅ **Do:**
- Use parallel jobs
- Cache dependencies
- Build only what changed
- Use matrix strategy

❌ **Don't:**
- Run unnecessary tests
- Build everything every time
- Skip caching

### 2. Fail Fast

✅ **Do:**
- Run quick checks first
- Fail on first error
- Stop pipeline on critical failures

❌ **Don't:**
- Continue on errors
- Hide failures
- Ignore test failures

### 3. Security

✅ **Do:**
- Scan for vulnerabilities
- Use secrets for sensitive data
- Limit permissions
- Review dependencies

❌ **Don't:**
- Commit secrets
- Use admin permissions
- Skip security scans

### 4. Versioning

✅ **Do:**
- Use semantic versioning
- Tag releases
- Document changes
- Create releases

❌ **Don't:**
- Use random versions
- Deploy without tags
- Skip release notes

### 5. Monitoring

✅ **Do:**
- Monitor deployments
- Set up alerts
- Track metrics
- Log everything

❌ **Don't:**
- Deploy blindly
- Ignore errors
- Skip monitoring

---

## Troubleshooting

### Common Issues

#### 1. Pipeline Fails on Build

**Problem:** Build job fails

**Solutions:**
```bash
# Check build logs
# Look for compilation errors
# Verify .NET SDK version matches
# Check for missing dependencies
```

#### 2. Docker Build Fails

**Problem:** Docker image build fails

**Solutions:**
```bash
# Check Dockerfile syntax
# Verify build context
# Check for missing files
# Verify base image exists
```

#### 3. Tests Fail

**Problem:** Test job fails

**Solutions:**
```bash
# Run tests locally first
# Check test configuration
# Verify test data
# Check for flaky tests
```

#### 4. Deployment Fails

**Problem:** Deployment job fails

**Solutions:**
```bash
# Check environment configuration
# Verify secrets are set
# Check network connectivity
# Verify permissions
```

#### 5. Slow Pipeline

**Problem:** Pipeline takes too long

**Solutions:**
```bash
# Enable caching
# Use parallel jobs
# Build only changed services
# Optimize Docker builds
```

### Debugging Tips

1. **Check Logs:**
   - Click on failed job
   - Expand failed step
   - Read error messages

2. **Run Locally:**
   - Reproduce issue locally
   - Test commands manually
   - Verify configuration

3. **Use Artifacts:**
   - Download build artifacts
   - Inspect generated files
   - Compare with expected output

4. **Enable Debug Logging:**
   ```yaml
   - name: Debug
     run: |
       echo "::debug::Debug information"
       env | sort
   ```

---

## Advanced Topics

### Custom Actions

Create reusable actions in `.github/actions/`:

```yaml
# .github/actions/build-dotnet/action.yml
name: 'Build .NET'
inputs:
  solution:
    required: true
runs:
  using: 'composite'
  steps:
    - run: dotnet build ${{ inputs.solution }}
      shell: bash
```

### Workflow Reuse

```yaml
# Reusable workflow
on:
  workflow_call:
    inputs:
      environment:
        required: true
jobs:
  deploy:
    uses: ./.github/workflows/deploy.yml
    with:
      environment: ${{ inputs.environment }}
```

### Conditional Deployments

```yaml
deploy:
  if: |
    github.ref == 'refs/heads/main' &&
    github.event.pull_request.merged == true
```

### Matrix with Exclude

```yaml
strategy:
  matrix:
    service: [service1, service2, service3]
    exclude:
      - service: service3
        condition: false
```

---

## Quick Reference

### Pipeline Status Badge

Add to README.md:
```markdown
![CI](https://github.com/your-username/your-repo/workflows/CI/badge.svg)
![CD](https://github.com/your-username/your-repo/workflows/CD/badge.svg)
```

### Manual Trigger

```bash
# Via GitHub UI:
# Actions → Manual Deployment → Run workflow

# Via GitHub CLI:
gh workflow run manual-deploy.yml -f environment=staging -f version=v1.0.0
```

### View Logs

```bash
# Via GitHub UI:
# Actions → Select workflow run → View logs

# Via GitHub CLI:
gh run view --log
```

---

## Next Steps

1. ✅ **Enable GitHub Actions** in repository settings
2. ✅ **Push code** to trigger CI pipeline
3. ✅ **Monitor pipeline** in Actions tab
4. ✅ **Set up environments** for staging/production
5. ✅ **Configure secrets** if needed
6. ✅ **Test deployment** to staging
7. ✅ **Create first release** with version tag

---

## Conclusion

You now understand:
- ✅ What CI/CD is and why it's important
- ✅ How GitHub Actions workflows work
- ✅ How to set up and configure pipelines
- ✅ How to optimize builds
- ✅ How to deploy safely
- ✅ How to troubleshoot issues

**Happy CI/CDing!** 🚀

