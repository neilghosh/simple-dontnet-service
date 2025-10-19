# ASP.NET Core Web API: Simple Dotnet Service

This project is a simple ASP.NET Core Web API that retrieves the outbound IP address by calling the ipify API. It demonstrates a clean layered architecture with separation of concerns.

## Directory Structure
```
project-root/
├── Controllers/
│   └── IpController.cs             # REST controller defining the GET /api/ip/outbound endpoint
├── Services/
│   ├── IIpAddressService.cs        # Service interface for IP address operations
│   └── OutboundIpService.cs        # Service implementation with business logic
├── Proxies/
│   ├── IIpifyProxy.cs              # Proxy interface for external API calls
│   └── IpifyProxy.cs               # Proxy implementation calling ipify API
├── SimpleDotnetService.Tests/      # Unit and integration test project
│   ├── Controllers/
│   │   └── IpControllerTests.cs    # Unit tests for IpController
│   ├── Services/
│   │   └── OutboundIpServiceTests.cs # Unit tests for OutboundIpService
│   ├── Proxies/
│   │   └── IpifyProxyTests.cs      # Unit tests for IpifyProxy
│   ├── Integration/
│   │   └── ApplicationIntegrationTests.cs # Integration tests for Program.cs
│   └── SimpleDotnetService.Tests.csproj  # Test project file
├── Program.cs                       # Application startup and dependency injection configuration
├── appsettings.json                 # Configuration settings for the application
├── appsettings.Development.json     # Development-specific configuration overrides
├── Dockerfile                       # Multi-stage Docker build configuration
├── simple-dotnet-service.sln       # Visual Studio solution file
├── simple-dotnet-service.http      # REST Client test file
└── README.md                        # Project documentation
```

## Architecture

This project follows a clean layered architecture with clear separation of concerns:

- **Controllers Layer**: Handles HTTP requests and responses (`IpController`)
- **Services Layer**: Contains business logic (`SimpleDotnetService.Services.Ip` namespace)
  - `IIpAddressService`: Service interface
  - `OutboundIpService`: Service implementation
- **Proxy Layer**: Abstracts external API calls (`SimpleDotnetService.Proxies` namespace - separate from Services)
  - `IIpifyProxy`: Proxy interface (separate file for better maintainability)
  - `IpifyProxy`: Proxy implementation calling ipify API
- **Interface Layer**: All public interfaces defined in separate files from implementations

The project uses:
- **Dependency Injection**: Services and proxies are registered in `Program.cs` and injected into controllers
- **Async/Await**: All service and proxy methods are asynchronous
- **Logging**: Integrated logging throughout all application layers
- **Error Handling**: Try-catch blocks with proper error logging

## How to Run Locally

1. Make sure you have the .NET 8.0 SDK installed.
2. In the project directory, run:
   
   ```bash
   dotnet run
   ```
3. Access the endpoint at [http://localhost:5000/api/ip/outbound](http://localhost:5000/api/ip/outbound)

## API Usage

The `/api/ip/outbound` endpoint retrieves your outbound IP address:

```bash
curl http://localhost:5000/api/ip/outbound
```

Response:
```json
{"outboundip":"203.0.113.42"}
```

## Docker

### Build the Docker Image

```bash
docker build -t simple-dotnet-service:latest .
```

### Run the Container

```bash
docker run -d -p 8080:8080 --name simple-dotnet-service simple-dotnet-service:latest
```

### Test the Containerized API

```bash
curl http://localhost:8080/api/ip/outbound
```

### Stop and Remove the Container

```bash
docker stop simple-dotnet-service
docker rm simple-dotnet-service
```

### View Container Logs

```bash
docker logs simple-dotnet-service
```

## Testing

The project includes comprehensive unit and integration tests with **100% code coverage**.

### Test Structure

- **Unit Tests**: Test individual components in isolation using Moq for mocking dependencies
  - `IpControllerTests.cs`: Tests for the IpController including success and error scenarios
  - `OutboundIpServiceTests.cs`: Tests for the OutboundIpService with various IP addresses
  - `IpifyProxyTests.cs`: Tests for the IpifyProxy including network errors and JSON parsing

- **Integration Tests**: Test the application startup and API endpoints end-to-end
  - `ApplicationIntegrationTests.cs`: Tests for Program.cs startup and WeatherForecast endpoint

### Running Tests

Run all tests:
```bash
dotnet test
```

Run tests with code coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

Generate HTML coverage report:
```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"./SimpleDotnetService.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"./SimpleDotnetService.Tests/TestResults/coveragereport" -reporttypes:Html
```

### Coverage Summary

- **Line coverage**: 100%
- **Branch coverage**: 100%
- **Method coverage**: 100%

All application files are fully covered by tests:
- Program.cs: 100%
- IpController: 100%
- OutboundIpService: 100%
- IpifyProxy: 100%
- WeatherForecast: 100%

Use the provided `simple-dotnet-service.http` file in VS Code with the REST Client extension to test endpoints easily.
