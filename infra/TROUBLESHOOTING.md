# Docker Build Troubleshooting Guide

This guide helps diagnose and resolve common Docker build issues for the TaskTracker application.

## Issues Fixed

### 1. Package Version Compatibility ✅
**Problem**: TaskTracker.Application had incompatible package versions:
- `Microsoft.AspNetCore.Identity` was version 2.2.0 (incompatible with .NET 8)
- `System.IdentityModel.Tokens.Jwt` was version 7.0.0 (should be 8.0.0)

**Solution**: Updated to version 8.0.0 for all packages to match .NET 8.

### 2. Incomplete Solution File ✅
**Problem**: `banayan-task-tracker.sln` only included TaskTracker.Domain project.

**Solution**: Added all projects to the solution file for proper dependency resolution.

### 3. Improved Dockerfile Strategy ✅
**Problem**: Individual project restore might miss cross-project dependencies.

**Solution**: 
- Copy solution file first
- Restore using the solution file with `--verbosity normal` for better error visibility
- This ensures all project references are resolved correctly

## Verification Steps

### Test Package Restore Locally
```bash
# From repository root
dotnet restore banayan-task-tracker.sln --verbosity normal
```

### Test Individual Project Build
```bash
# Test API build
dotnet build src/TaskTracker.Api/TaskTracker.Api.csproj

# Test Worker build
dotnet build src/TaskTracker.Worker/TaskTracker.Worker.csproj
```

### Test Docker Build (Single Service)
```bash
# From repository root
docker build -f infra/Dockerfile.api -t tasktracker-api-test .
docker build -f infra/Dockerfile.worker -t tasktracker-worker-test .
```

## Common Docker Issues & Solutions

### Issue: "Package 'X' is incompatible with netcoreapp/net8.0"
**Cause**: Old package versions from previous .NET versions
**Solution**: Update package versions to match target framework (8.0.0 for .NET 8)

### Issue: "Project reference could not be resolved"
**Cause**: Missing projects in solution file or incorrect paths
**Solution**: Ensure all projects are in the solution file and paths are correct

### Issue: "Unable to resolve service for type"
**Cause**: DI registration issues or missing service dependencies
**Solution**: Check Program.cs service registrations and ensure all dependencies are registered

### Issue: "Connection string not found"
**Cause**: Environment variable or configuration missing
**Solution**: Verify TASKTRACKER_CONNECTION_STRING is set in docker-compose.yml

### Issue: Build context too large
**Cause**: Including unnecessary files in Docker build context
**Solution**: Check .dockerignore file excludes bin/, obj/, node_modules/, etc.

## Debug Commands

### View Docker Build Output
```bash
# Build with detailed output
docker build -f infra/Dockerfile.api -t tasktracker-api --progress=plain --no-cache .
```

### Inspect Build Layers
```bash
# Run intermediate build stage for debugging
docker build -f infra/Dockerfile.api --target build -t tasktracker-api-build .
docker run -it tasktracker-api-build /bin/bash
```

### Check Container Logs
```bash
# View container logs
docker logs tasktracker-api
docker logs tasktracker-worker
```

## Environment Variable Verification

### Required Environment Variables
- `TASKTRACKER_CONNECTION_STRING`: Database connection string
- `ASPNETCORE_ENVIRONMENT`: Should be "Development" for local Docker
- `VITE_API_BASE_URL`: Frontend API endpoint (for web service)

### Check Environment Variables in Container
```bash
# Exec into running container
docker exec -it tasktracker-api /bin/bash
env | grep TASKTRACKER
```