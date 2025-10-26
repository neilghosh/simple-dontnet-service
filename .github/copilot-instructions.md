<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This is an ASP.NET Core Web API project using layered architecture with dependency injection. Use C# best practices for REST endpoints. The main GET endpoint is at /api/name and returns a name in JSON format.

## Architecture Guidelines

- Follow separation of concerns with Controllers and Services layers
- Use dependency injection for all services
- Implement interfaces for all service classes
- Use async/await patterns for service methods
- Include proper logging and error handling
- Register services in Program.cs using appropriate lifetime (Scoped, Singleton, Transient)
- **Always place interface definitions in separate files** from their implementations
  - Example: `IIpifyProxy.cs` contains only the interface definition
  - Example: `IpifyProxy.cs` contains only the implementation class
  - This improves maintainability and follows single responsibility principle

## C# Coding Style Guidelines

Follow Microsoft's official C# coding style guidelines (https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names):

### Naming Conventions:
- **PascalCase**: Classes, interfaces, methods, properties, public fields, namespaces
- **camelCase**: Private fields, local variables, method parameters
- **Interface names**: Start with capital 'I' (e.g., INameService)
- **Private fields**: Use camelCase without underscores (e.g., logger, not _logger)
- **Static fields**: Use 's_' prefix (e.g., s_instance)
- **Thread static fields**: Use 't_' prefix (e.g., t_threadLocal)
- **Constants**: Use PascalCase (e.g., DefaultTimeout)

### General Guidelines:
- Use meaningful and descriptive names
- Prefer clarity over brevity
- Avoid abbreviations except for widely known ones
- Avoid single-letter names (except for simple loop counters)
- Use 'this' keyword when accessing instance members to avoid ambiguity
- Do not create MD files unless explicitly requested by the user

## Updating live documentation in README.md

**CRITICAL: This step is MANDATORY for ANY project structure or architecture changes.**

When making ANY of the following changes, README.md MUST be updated immediately:
- Adding or removing folders/files
- Renaming classes, interfaces, or files
- Moving code to different namespaces
- Changing folder structure
- Adding new layers or components
- Modifying architecture patterns

Update the directory structure section with:
- Current folder hierarchy showing all directories
- File names and their purposes
- Updated namespace information if changed

This ensures documentation stays in sync with the actual codebase and prevents confusion.

## Test Summary Documents

**DO NOT** create test summary or test result documents (e.g., TEST_RESULTS.md, TESTING_SUMMARY.md) unless explicitly asked by the user. Testing results should be communicated directly in the conversation. Only create such documentation if the user specifically requests it.

## Build and Test After Major Refactors

Whenever there is a major refactor (namespace changes, service layer modifications, architecture updates, etc.), follow these steps to ensure everything works:

### 1. Build the Project
```pwsh
dotnet build simple-dotnet-service.csproj
```
Verify build succeeds with no errors or warnings.

### 2. Run the Local Service
```pwsh
dotnet run
```
The service should start and listen on `http://localhost:5000`.

### 3. Test API Endpoints

**Test without parameters (default value):**
```pwsh
Invoke-WebRequest -Uri "http://localhost:5000/api/name" -UseBasicParsing
```
Expected: `{"name":"DefaultUser"}` with status 200 OK

**Test with query parameters:**
```pwsh
Invoke-WebRequest -Uri "http://localhost:5000/api/name?name=TestValue" -UseBasicParsing
```
Expected: `{"name":"TestValue"}` with status 200 OK

### 4. Docker Build and Test (Optional but Recommended)
```pwsh
# Build Docker image
docker build -t simple-dotnet-service:latest .

# Run Docker container
docker run -d -p 8080:8080 --name simple-dotnet-service simple-dotnet-service:latest

# Test API in Docker
Invoke-WebRequest -Uri "http://localhost:8080/api/name" -UseBasicParsing
Invoke-WebRequest -Uri "http://localhost:8080/api/name?name=Docker" -UseBasicParsing

# Cleanup
docker stop simple-dotnet-service
docker rm simple-dotnet-service
```

### 5. Verify All Pass
✅ Build completes successfully
✅ Service starts without errors
✅ API returns correct JSON responses
✅ Default values work as expected
✅ Query parameters are properly bound
✅ HTTP status codes are correct (200 OK)
✅ Docker image builds and runs successfully (if applicable)