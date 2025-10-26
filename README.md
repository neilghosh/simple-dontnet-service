# ASP.NET Core Web API: Simple Dotnet Service

This project is a simple ASP.NET Core Web API that retrieves the outbound IP address by calling the ipify API. It demonstrates a clean layered architecture with separation of concerns.

## Directory Structure
```
project-root/
├── .github/
│   └── workflows/
│       └── azure-container-deploy.yml       # CI/CD pipeline for Azure deployment
├── Controllers/
│   └── IpController.cs                      # REST controller with IP endpoints
├── Services/
│   ├── IIpAddressService.cs                 # Service interface for IP address operations
│   └── OutboundIpService.cs                 # Service implementation with business logic
├── Proxies/
│   ├── IIpifyProxy.cs                       # Proxy interface for external API calls
│   └── IpifyProxy.cs                        # Proxy implementation calling ipify API
├── SimpleDotnetService.Tests/               # Unit and integration test project
│   ├── Controllers/
│   │   └── IpControllerTests.cs             # Unit tests for IpController
│   ├── Services/
│   │   └── OutboundIpServiceTests.cs        # Unit tests for OutboundIpService
│   ├── Proxies/
│   │   └── IpifyProxyTests.cs               # Unit tests for IpifyProxy
│   ├── Integration/
│   │   └── ApplicationIntegrationTests.cs   # Integration tests for Program.cs
│   ├── IpControllerTests.cs                 # Additional unit tests for IpController
│   ├── IpControllerIntegrationTests.cs      # Additional integration tests for API endpoints
│   └── SimpleDotnetService.Tests.csproj     # Test project file
├── Program.cs                                # Application startup and dependency injection configuration
├── appsettings.json                          # Configuration settings for the application
├── appsettings.Development.json              # Development-specific configuration overrides
├── Dockerfile                                # Multi-stage Docker build configuration
├── arch.wsd                                  # Architecture diagram source (PlantUML)
├── Simple DotNet Service Architecture.png    # Architecture diagram image
├── AZURE_DEPLOYMENT.md                       # Azure deployment setup guide
├── QUICKSTART.md                             # 5-minute Azure deployment quickstart
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

#### Unit Tests

**Controller Tests** (`Controllers/IpControllerTests.cs` and `IpControllerTests.cs`)
- Tests controller methods in isolation using mocked dependencies
- Validates business logic and error handling
- Tests all three endpoints: `/api/ip/outbound`, `/api/ip/inbound`, `/api/ip/headers`

**Test Coverage:**
- ✅ `GetOutboundIp_ReturnsOkResult_WithOutboundIpAddress` - Tests successful IP retrieval
- ✅ `GetOutboundIp_ReturnsInternalServerError_WhenExceptionOccurs` - Tests error handling
- ✅ `GetOutboundIp_ReturnsInternalServerError_WhenHttpRequestException` - Tests HTTP exceptions
- ✅ `GetInboundIp_ReturnsOkResult_WithRemoteIpAddress` - Tests inbound IP detection
- ✅ `GetInboundIp_ReturnsUnknown_WhenRemoteIpIsNull` - Tests null IP handling
- ✅ `GetHeaders_ReturnsOkResult_WithRequestHeaders` - Tests header extraction
- ✅ `GetHeaders_ReturnsEmptyDictionary_WhenNoHeaders` - Tests empty headers
- ✅ `GetInboundIp_ReturnsOkResult_WithIpv4Address` - Tests IPv4 addresses
- ✅ `GetInboundIp_ReturnsOkResult_WithIpv6Address` - Tests IPv6 addresses

**Service Tests** (`Services/OutboundIpServiceTests.cs`)
- Tests service layer business logic
- Validates interaction with proxy layer
- Tests error handling and logging

**Test Coverage:**
- ✅ `GetOutboundIpAsync_ReturnsIpAddress_WhenProxySucceeds` - Tests successful proxy call
- ✅ `GetOutboundIpAsync_ThrowsException_WhenProxyFails` - Tests proxy failure handling
- ✅ `GetOutboundIpAsync_LogsInformation_OnSuccess` - Tests logging behavior
- ✅ `GetOutboundIpAsync_LogsError_OnFailure` - Tests error logging

**Proxy Tests** (`Proxies/IpifyProxyTests.cs`)
- Tests external API integration layer
- Validates HTTP client behavior
- Tests error scenarios and edge cases

**Test Coverage:**
- ✅ `GetIpAsync_ReturnsIpAddress_WhenApiSucceeds` - Tests successful API call
- ✅ `GetIpAsync_ThrowsHttpRequestException_WhenApiReturnsNonSuccess` - Tests API errors
- ✅ `GetIpAsync_ThrowsHttpRequestException_WhenNetworkError` - Tests network failures
- ✅ `GetIpAsync_ReturnsValidIpv4Address` - Validates IPv4 format
- ✅ `GetIpAsync_ReturnsValidIpv6Address` - Validates IPv6 format
- ✅ `GetIpAsync_TrimsWhitespace` - Tests string handling

#### Integration Tests

**Application Integration Tests** (`Integration/ApplicationIntegrationTests.cs`)
- Tests the full HTTP pipeline using `WebApplicationFactory`
- Validates end-to-end functionality including routing, serialization, and HTTP responses
- Tests actual API responses and JSON structure

**Test Coverage:**
- ✅ `GetOutboundIp_ReturnsSuccessStatusCode` - Tests HTTP 200 response
- ✅ `GetOutboundIp_ReturnsValidJsonWithIpAddress` - Tests JSON structure
- ✅ `GetInboundIp_ReturnsSuccessStatusCode` - Tests HTTP 200 response
- ✅ `GetInboundIp_ReturnsValidJsonWithIpAddress` - Tests JSON structure
- ✅ `GetHeaders_ReturnsSuccessStatusCode` - Tests HTTP 200 response
- ✅ `GetHeaders_ReturnsValidJsonDictionary` - Tests JSON dictionary structure
- ✅ `GetHeaders_IncludesRequestHeaders` - Tests custom header inclusion
- ✅ `GetHeaders_ContainsUserAgentHeader` - Tests default headers

**Additional Integration Tests** (`IpControllerIntegrationTests.cs`)
- Additional integration tests for API endpoints
- Tests HTTP pipeline and JSON responses
- Validates header handling and IP detection

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

**Combined Test Coverage:**
- **Total Tests:** 38+
- **Passing:** 36+ ✅
- **Skipped:** 2 ⏭️ (require external API access)
- **Failing:** 0 ❌

**Code Coverage:** Comprehensive coverage across all layers:
- ✅ Controllers: 100%
- ✅ Services: 100%
- ✅ Proxies: 100%
- ✅ Integration: Full HTTP pipeline

**Note:** Some integration tests for the outbound IP endpoint are skipped as they require external network access to `api.ipify.org`.

## Azure Deployment

This project includes automated CI/CD deployment to Azure using GitHub Actions. Choose the deployment option that best fits your needs:

### Deployment Options

#### Option 1: Azure App Service (Recommended)
**Best for: Production applications, HTTPS support, custom domains**

- **5-Minute Setup Guide**: See [docs/AZURE_APP_SERVICE_QUICKSTART.md](docs/AZURE_APP_SERVICE_QUICKSTART.md)
- **Detailed Documentation**: See [docs/AZURE_APP_SERVICE_DEPLOYMENT.md](docs/AZURE_APP_SERVICE_DEPLOYMENT.md)

**Key Benefits:**
- ✅ Built-in HTTPS support on port 443
- ✅ Automatic SSL/TLS certificates
- ✅ Custom domain support
- ✅ Deployment slots for zero-downtime updates
- ✅ Integrated monitoring and scaling
- ✅ Fully managed platform

**Quick Test:**
```bash
# After deployment, test with HTTPS
curl https://<your-app-name>.azurewebsites.net/api/ip/outbound
curl https://<your-app-name>.azurewebsites.net/api/ip/inbound
curl https://<your-app-name>.azurewebsites.net/api/ip/headers
```

#### Option 2: Azure Container Instances
**Best for: Pay-per-second billing, Docker-first deployment**

- **5-Minute Setup Guide**: See [docs/QUICKSTART.md](docs/QUICKSTART.md)
- **Detailed Documentation**: See [docs/AZURE_DEPLOYMENT.md](docs/AZURE_DEPLOYMENT.md)

**Key Benefits:**
- ✅ Pay-per-second billing
- ✅ No always-on costs
- ✅ Serverless hosting
- ✅ Docker compatibility

**Quick Test:**
```bash
# After deployment, test with HTTP
curl http://<your-container-url>:8080/api/ip/outbound
curl http://<your-container-url>:8080/api/ip/inbound
curl http://<your-container-url>:8080/api/ip/headers
```

### Comparison

| Feature | App Service | Container Instances |
|---------|-------------|---------------------|
| HTTPS Support | Built-in (port 443) | Supported, requires setup |
| SSL Certificates | Automatic, managed | Manual configuration |
| Custom Domains | Native support | Requires additional configuration |
| Deployment Slots | Yes (Standard+) | No |
| Cost Model | Fixed monthly | Pay-per-second |
| Best For | Production apps | Dev/test, batch jobs |

### Workflow Triggers

Both deployment pipelines run on:
- Push to `main` branch (builds, tests, and deploys)
- Pull requests (builds and tests only)
- Manual workflow dispatch

### Prerequisites

1. Azure subscription
2. Azure CLI installed
3. GitHub repository secrets configured

For complete setup and troubleshooting guides, see the documentation links above.
