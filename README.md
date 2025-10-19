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

Use the provided `simple-dotnet-service.http` file in VS Code with the REST Client extension to test endpoints easily.
