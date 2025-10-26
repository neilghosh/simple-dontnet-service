# ASP.NET Core Web API: Simple Dotnet Service

This project is a simple ASP.NET Core Web API that retrieves the outbound IP address by calling the ipify API. It demonstrates a clean layered architecture with separation of concerns.

## Directory Structure
```
project-root/
├── Controllers/
│   └── IpController.cs                      # REST controller with IP endpoints
├── Services/
│   ├── IIpAddressService.cs                 # Service interface for IP address operations
│   └── OutboundIpService.cs                 # Service implementation with business logic
├── Proxies/
│   ├── IIpifyProxy.cs                       # Proxy interface for external API calls
│   └── IpifyProxy.cs                        # Proxy implementation calling ipify API
├── SimpleDotnetService.Tests/               # Test project
│   ├── IpControllerTests.cs                 # Unit tests for IpController
│   ├── IpControllerIntegrationTests.cs      # Integration tests for API endpoints
│   └── SimpleDotnetService.Tests.csproj     # Test project file
├── Program.cs                                # Application startup and dependency injection configuration
├── appsettings.json                          # Configuration settings for the application
├── appsettings.Development.json              # Development-specific configuration overrides
├── Dockerfile                                # Multi-stage Docker build configuration
├── simple-dotnet-service.sln                # Visual Studio solution file
├── simple-dotnet-service.http               # REST Client test file
└── README.md                                 # Project documentation
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

The API provides three endpoints:

### 1. Get Outbound IP Address
Retrieves your outbound IP address by calling an external API (ipify):

```bash
curl http://localhost:5000/api/ip/outbound
```

Response:
```json
{"outboundip":"203.0.113.42"}
```

### 2. Get Inbound (Remote) IP Address
Returns the IP address of the client making the request:

```bash
curl http://localhost:5000/api/ip/inbound
```

Response:
```json
{"inboundip":"::1"}
```

### 3. Get Request Headers
Lists all HTTP headers sent with the request:

```bash
curl http://localhost:5000/api/ip/headers
```

Response:
```json
{
  "Host": "localhost:5000",
  "User-Agent": "curl/7.68.0",
  "Accept": "*/*"
}
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

The project includes comprehensive tests using xUnit framework with both unit and integration tests.

### Running Tests

To run all tests:

```bash
dotnet test
```

To run tests with detailed output:

```bash
dotnet test -v detailed
```

To run tests with coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

The test suite includes:

#### Unit Tests (`IpControllerTests.cs`)
- Tests controller methods in isolation using mocked dependencies
- Validates business logic and error handling
- Tests all three endpoints: `/api/ip/outbound`, `/api/ip/inbound`, `/api/ip/headers`

**Test Coverage:**
- ✅ `GetOutboundIp_ReturnsOkResult_WithOutboundIpAddress` - Tests successful IP retrieval
- ✅ `GetOutboundIp_ReturnsInternalServerError_WhenExceptionOccurs` - Tests error handling
- ✅ `GetInboundIp_ReturnsOkResult_WithRemoteIpAddress` - Tests inbound IP detection
- ✅ `GetInboundIp_ReturnsUnknown_WhenRemoteIpIsNull` - Tests null IP handling
- ✅ `GetHeaders_ReturnsOkResult_WithRequestHeaders` - Tests header extraction
- ✅ `GetHeaders_ReturnsEmptyDictionary_WhenNoHeaders` - Tests empty headers

#### Integration Tests (`IpControllerIntegrationTests.cs`)
- Tests the full HTTP pipeline using `WebApplicationFactory`
- Validates end-to-end functionality including routing, serialization, and HTTP responses
- Tests actual API responses and JSON structure

**Test Coverage:**
- ⏭️ `GetOutboundIp_ReturnsSuccessStatusCode` - Skipped (requires external API)
- ⏭️ `GetOutboundIp_ReturnsValidJsonWithIpAddress` - Skipped (requires external API)
- ✅ `GetInboundIp_ReturnsSuccessStatusCode` - Tests HTTP 200 response
- ✅ `GetInboundIp_ReturnsValidJsonWithIpAddress` - Tests JSON structure
- ✅ `GetHeaders_ReturnsSuccessStatusCode` - Tests HTTP 200 response
- ✅ `GetHeaders_ReturnsValidJsonDictionary` - Tests JSON dictionary structure
- ✅ `GetHeaders_IncludesRequestHeaders` - Tests custom header inclusion
- ✅ `GetHeaders_ContainsUserAgentHeader` - Tests default headers

### Test Results Summary
- **Total Tests:** 14
- **Passing:** 12 ✅
- **Skipped:** 2 ⏭️ (require external API access)
- **Failing:** 0 ❌

**Note:** Two integration tests for the outbound IP endpoint are skipped as they require external network access to `api.ipify.org`.


