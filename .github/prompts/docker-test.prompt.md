# Docker Build and Test

Build the Docker image, run the container, test all endpoints, and cleanup.

## Pre-flight Checks

Before starting, verify:
1. **Docker is installed**: Run `docker --version` to confirm Docker is available
2. **Docker daemon is running**: Run `docker info` to verify the Docker daemon is responsive
3. **Port 8080 is available**: Run `lsof -i :8080` to check if port 8080 is in use
4. **No conflicting container**: Run `docker ps -a --filter name=simple-dotnet-service` to check for existing containers with the same name. If found, stop and remove them first.

## Build Steps

1. Build the Docker image:
   ```bash
   docker build -t simple-dotnet-service .
   ```

2. Run the container:
   ```bash
   docker run -d --name simple-dotnet-service -p 8080:8080 simple-dotnet-service
   ```

3. Wait for startup (3 seconds)

## Test Endpoints

Test all available endpoints:
- **Health**: `curl http://localhost:8080/health`
- **Outbound IP**: `curl http://localhost:8080/api/ip/outbound`
- **Inbound IP**: `curl http://localhost:8080/api/ip/inbound`
- **Headers**: `curl http://localhost:8080/api/ip/headers`

## Cleanup

After testing, clean up:
```bash
docker stop simple-dotnet-service && docker rm simple-dotnet-service
```
