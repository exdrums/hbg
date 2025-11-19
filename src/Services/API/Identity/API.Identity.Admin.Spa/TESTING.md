# Identity Admin SPA - Testing Guide

Complete guide for running all tests in the Identity Admin SPA project.

## Table of Contents

- [Quick Start](#quick-start)
- [Angular Unit Tests](#angular-unit-tests)
- [ASP.NET Core Integration Tests](#aspnet-core-integration-tests)
- [Docker Testing](#docker-testing)
- [Kubernetes Deployment Testing](#kubernetes-deployment-testing)
- [Continuous Integration](#continuous-integration)

## Quick Start

### Run All Tests (Linux/macOS)

```bash
cd src/Services/API/Identity/API.Identity.Admin.Spa
./run-tests.sh all
```

### Run All Tests (Windows PowerShell)

```powershell
cd src\Services\API\Identity\API.Identity.Admin.Spa
.\run-tests.ps1 -TestType all
```

### Run Specific Test Types

**Linux/macOS:**
```bash
./run-tests.sh angular    # Angular unit tests only
./run-tests.sh dotnet     # ASP.NET Core tests only
./run-tests.sh docker     # Docker build and health check only
```

**Windows PowerShell:**
```powershell
.\run-tests.ps1 angular   # Angular unit tests only
.\run-tests.ps1 dotnet    # ASP.NET Core tests only
.\run-tests.ps1 docker    # Docker build and health check only
```

## Angular Unit Tests

### Prerequisites

- Node.js 18+ installed
- npm installed

### Running Tests

**From project root:**
```bash
cd Client
npm install
npm test
```

**Watch mode (for development):**
```bash
npm test
# Tests will re-run on file changes
```

**Run with coverage:**
```bash
npm run test:coverage
```

### Test Files

Angular test files follow the `*.spec.ts` naming convention:

- `src/app/core/services/auth.service.spec.ts` - Authentication service tests
- `src/app/modules/clients/services/clients.service.spec.ts` - Clients service tests
- `src/app/modules/clients/client-form/client-form.component.spec.ts` - Client form component tests

### Writing New Tests

Create test files alongside the code they test:

```typescript
import { TestBed } from '@angular/core/testing';
import { MyService } from './my-service.service';

describe('MyService', () => {
  let service: MyService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MyService]
    });
    service = TestBed.inject(MyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
```

## ASP.NET Core Integration Tests

### Prerequisites

- .NET 9.0 SDK installed

### Running Tests

**From solution root:**
```bash
cd src/Services/API/Identity/API.Identity.Admin.Spa.Tests
dotnet test
```

**With detailed output:**
```bash
dotnet test --verbosity detailed
```

**With code coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Files

- `HealthCheckTests.cs` - Health check endpoint tests
- `ConfigurationControllerTests.cs` - Configuration API tests

### Writing New Tests

Integration tests use `WebApplicationFactory` for in-memory testing:

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class MyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Test_EndpointReturnsSuccess()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/my-endpoint");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Docker Testing

### Prerequisites

- Docker installed and running
- Sufficient disk space for image layers

### Build Docker Image

**From repository root:**
```bash
docker build -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
    -t exdrums/hbg-admin-spa:latest \
    .
```

### Run Container Locally

```bash
docker run -d -p 5796:80 \
    -e ASPNETCORE_ENVIRONMENT=Development \
    -e HBGIDENTITY=https://sts.hbg.lol \
    -e HBGIDENTITYADMINSPA=https://adminspa.hbg.lol \
    -e HBGIDENTITYADMINAPI=https://adminapi.hbg.lol \
    --name hbg-admin-spa-test \
    exdrums/hbg-admin-spa:latest
```

### Test Health Endpoint

```bash
# Wait for container to start
sleep 10

# Test health check
curl http://localhost:5796/health

# Test health check UI
curl http://localhost:5796/hc

# Test configuration endpoint
curl http://localhost:5796/configuration
```

### View Logs

```bash
docker logs hbg-admin-spa-test
```

### Cleanup

```bash
docker stop hbg-admin-spa-test
docker rm hbg-admin-spa-test
```

## Kubernetes Deployment Testing

### Prerequisites

- Kubernetes cluster (minikube, kind, or cloud provider)
- kubectl installed and configured
- Docker images built locally or pushed to registry

### Apply Kubernetes Manifests

**Apply ConfigMap:**
```bash
kubectl apply -f k8s/hbg-configmap.yaml
```

**Apply Secrets (if needed):**
```bash
kubectl apply -f k8s/hbg-secret.yaml
```

**Deploy Admin SPA:**
```bash
kubectl apply -f k8s/hbg-admin-spa.yaml
```

**Apply Ingress:**
```bash
kubectl apply -f k8s/hbg-ingress.yaml
```

### Verify Deployment

**Check pod status:**
```bash
kubectl get pods -l app=hbg-admin-spa
```

**Check service:**
```bash
kubectl get service hbg-admin-spa-service
```

**Check ingress:**
```bash
kubectl get ingress hbg-ingress
```

### View Logs

```bash
kubectl logs -l app=hbg-admin-spa -f
```

### Test Health Checks

**Port forward to test directly:**
```bash
kubectl port-forward service/hbg-admin-spa-service 8080:80
```

**In another terminal:**
```bash
curl http://localhost:8080/health
curl http://localhost:8080/hc
```

### Access via Ingress

**Add to /etc/hosts:**
```
127.0.0.1 adminspa.hbg.lol
```

**Access the application:**
```bash
# If using local ingress controller
kubectl port-forward --namespace=ingress-nginx service/ingress-nginx-controller 80:80 443:443

# Then browse to:
https://adminspa.hbg.lol
```

### Troubleshooting

**Pod won't start:**
```bash
kubectl describe pod -l app=hbg-admin-spa
kubectl logs -l app=hbg-admin-spa
```

**Health check failing:**
```bash
kubectl exec -it <pod-name> -- curl http://localhost:80/health
```

**Configuration issues:**
```bash
kubectl get configmap hbg-configmap -o yaml
kubectl exec -it <pod-name> -- env | grep HBG
```

### Cleanup Kubernetes Resources

```bash
kubectl delete -f k8s/hbg-admin-spa.yaml
```

## Continuous Integration

### GitHub Actions Example

Create `.github/workflows/admin-spa-tests.yml`:

```yaml
name: Identity Admin SPA Tests

on:
  push:
    branches: [ main, develop ]
    paths:
      - 'src/Services/API/Identity/API.Identity.Admin.Spa/**'
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'

    - name: Run Angular Tests
      run: |
        cd src/Services/API/Identity/API.Identity.Admin.Spa/Client
        npm install
        npm test -- --watch=false --browsers=ChromeHeadless

    - name: Run .NET Tests
      run: |
        cd src/Services/API/Identity/API.Identity.Admin.Spa.Tests
        dotnet test --verbosity normal

    - name: Build Docker Image
      run: |
        docker build -f src/Services/API/Identity/API.Identity.Admin.Spa/Dockerfile \
          -t exdrums/hbg-admin-spa:${{ github.sha }} \
          .
```

## Test Coverage

### View Angular Coverage

```bash
cd Client
npm run test:coverage
# Open coverage/index.html in browser
```

### View .NET Coverage

```bash
cd ../API.Identity.Admin.Spa.Tests
dotnet test --collect:"XPlat Code Coverage"
# Coverage reports in TestResults directory
```

## Performance Testing

### Load Testing with Apache Bench

```bash
# Test health endpoint
ab -n 1000 -c 10 http://localhost:5796/health

# Test configuration endpoint
ab -n 1000 -c 10 http://localhost:5796/configuration
```

### Expected Results

- Health endpoint: < 50ms response time
- Configuration endpoint: < 100ms response time
- Container startup: < 40 seconds
- Angular build time: < 2 minutes
- .NET build time: < 1 minute

## Best Practices

1. **Run tests before committing** - Use git hooks or manual checks
2. **Keep tests fast** - Unit tests should run in seconds
3. **Mock external dependencies** - Don't rely on external APIs in tests
4. **Test one thing at a time** - Each test should verify a single behavior
5. **Use descriptive test names** - Test names should explain what they verify
6. **Maintain test coverage** - Aim for >80% code coverage
7. **Update tests with code** - Tests are part of the codebase, not afterthoughts

## Support

For issues or questions:
- Check logs: `docker logs <container>` or `kubectl logs <pod>`
- Review test output for specific failures
- Verify environment variables are set correctly
- Ensure all dependencies are installed
