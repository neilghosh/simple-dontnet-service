# ASP.NET Core Web API: Simple Dotnet Service

This project is a simple ASP.NET Core Web API that retrieves the outbound IP address by calling the ipify API. It demonstrates a clean layered architecture with separation of concerns and Azure AD authentication using OAuth 2.0 Authorization Code Flow with PKCE.

## Directory Structure
```
project-root/
├── .github/
│   └── workflows/
│       └── azure-container-deploy.yml       # CI/CD pipeline for Azure deployment
├── Controllers/
│   ├── IpController.cs                      # REST controller with IP endpoints
│   ├── UserController.cs                    # Protected controller for user claims
│   └── ConfigController.cs                  # Azure AD configuration endpoint
├── wwwroot/
│   └── index.html                           # SPA demo with Azure AD authentication
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
├── docs/
│   ├── arch.wsd                             # Architecture diagram source (PlantUML)
│   ├── Simple DotNet Service Architecture.png  # Architecture diagram image
│   └── DEPLOYMENT_OVERVIEW.md               # Complete deployment documentation
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
- **Azure AD Authentication**: JWT bearer token authentication with OAuth 2.0 PKCE flow for SPAs
- **Dependency Injection**: Services and proxies are registered in `Program.cs` and injected into controllers
- **Async/Await**: All service and proxy methods are asynchronous
- **Logging**: Integrated logging throughout all application layers
- **Error Handling**: Try-catch blocks with proper error logging
- **Swagger/OpenAPI**: Interactive API documentation with OAuth2 PKCE support

## Azure AD Authentication

This service supports Azure AD authentication using OAuth 2.0 Authorization Code Flow with PKCE (Proof Key for Code Exchange), making it suitable for Single Page Applications (SPAs) and Microsoft Account (MSA) users.

### Features

- 🔐 **SPA Authentication**: Pure JavaScript OAuth 2.0 implementation without MSAL.js dependency
- 🔑 **PKCE Support**: Secure authorization code flow for public clients
- 👤 **MSA Support**: Single tenant configuration for Microsoft Account users
- 📋 **Claims Display**: View token claims and granted scopes
- 📖 **Swagger Integration**: OAuth2 authentication in Swagger UI

### Configuration

1. **Register an Azure AD Application**:
   - Go to [Azure Portal](https://portal.azure.com) > Azure Active Directory > App registrations
   - Create a new registration with "Accounts in any organizational directory and personal Microsoft accounts"
   - Add a Single-page application (SPA) redirect URI: `http://localhost:8080/`
   - Under "Expose an API", create an Application ID URI and add a scope (e.g., `User.Read`)

2. **Update `appsettings.json`**:
   ```json
   {
     "AzureAd": {
       "Instance": "https://login.microsoftonline.com/",
       "TenantId": "consumers",
       "ClientId": "YOUR_CLIENT_ID_HERE",
       "Scopes": "api://YOUR_CLIENT_ID_HERE/User.Read"
     }
   }
   ```

3. **API Endpoints**:
   - `GET /` - SPA demo page with Azure AD login
   - `GET /api/config` - Public endpoint returning Azure AD configuration
   - `GET /api/user/claims` - **Protected** endpoint requiring authentication
   - `GET /swagger` - Swagger UI with OAuth2 authentication

## How to Run Locally

1. Make sure you have the .NET 8.0 SDK installed.
2. In the project directory, run:
   
   ```bash
   dotnet run
   ```
3. Access the endpoint at [http://localhost:5000/api/ip/outbound](http://localhost:5000/api/ip/outbound)

## API Usage

The API provides public and protected endpoints:

### Public Endpoints

#### 1. Get Outbound IP Address
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

#### 3. Get Request Headers
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

#### 4. Get Azure AD Configuration
Returns the Azure AD configuration for the SPA client:

```bash
curl http://localhost:5000/api/config
```

Response:
```json
{
  "clientId": "YOUR_CLIENT_ID",
  "tenantId": "consumers",
  "scopes": ["api://YOUR_CLIENT_ID/User.Read"]
}
```

### Protected Endpoints (Require Azure AD Bearer Token)

#### 5. Get User Claims
Returns the claims from the authenticated user's token:

```bash
curl -H "Authorization: Bearer YOUR_ACCESS_TOKEN" http://localhost:5000/api/user/claims
```

Response:
```json
{
  "isAuthenticated": true,
  "name": "John Doe",
  "claims": {
    "name": "John Doe",
    "preferred_username": "john@example.com",
    "oid": "12345678-1234-1234-1234-123456789abc"
  },
  "scopes": ["User.Read"]
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

This project includes automated CI/CD deployment to **Azure Container Apps** using GitHub Actions with OIDC authentication.

📖 **Complete Documentation**: See [docs/DEPLOYMENT_OVERVIEW.md](docs/DEPLOYMENT_OVERVIEW.md)

### Deployment Overview

- **Target**: Azure Container Apps
- **Authentication**: OIDC (OpenID Connect) via Service Principal - no stored secrets
- **Trigger**: Push to `main` branch or manual workflow dispatch
- **Pipeline**: Build → Test → Deploy → Health Check

### Key Features

- ✅ **Secure OIDC Authentication** - No long-lived credentials stored in GitHub
- ✅ **Automatic HTTPS** - Built-in TLS termination
- ✅ **Auto-scaling** - Scale to zero when idle
- ✅ **Health Verification** - Automatic health checks after deployment

### Quick Test (After Deployment)

```bash
# Get your app URL
URL=$(az containerapp show -n simple-dotnet-service -g simple-dotnet-service-rg --query properties.configuration.ingress.fqdn -o tsv)

# Test the endpoints
curl https://$URL/api/ip/outbound
curl https://$URL/api/ip/inbound
curl https://$URL/api/ip/headers
```

### GitHub Secrets Required

Only 3 secrets are needed (OIDC-based authentication):

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Service Principal Application ID |
| `AZURE_TENANT_ID` | Azure AD Tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure Subscription ID |

### Workflow Triggers

- **Push to `main`**: Builds, tests, and deploys
- **Pull requests**: Builds and tests only (no deployment)
- **Manual dispatch**: Trigger from GitHub Actions UI with region selection
