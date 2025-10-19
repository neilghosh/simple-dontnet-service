# ASP.NET Core Web API: Simple Dotnet Service

This project is a simple ASP.NET Core Web API with a GET endpoint at `/api/name` that returns a name in JSON format.

## Directory Structure
```
project-root/
├── Controllers/
│   └── NameController.cs       # REST controller defining the GET /api/name endpoint
├── Services/
│   ├── INameService.cs         # Service interface for name operations
│   └── NameService.cs          # Service implementation with business logic
├── Program.cs                   # Application startup and dependency injection configuration
├── appsettings.json             # Configuration settings for the application
├── appsettings.Development.json # Development-specific configuration overrides
├── Dockerfile                   # Multi-stage Docker build configuration
├── .dockerignore                # Files excluded from Docker build context
├── .csproj                      # Project file with NuGet dependencies and build settings
└── README.md                    # Project documentation
```

## Architecture

This project follows a layered architecture with separation of concerns:

- **Controllers Layer**: Handles HTTP requests and responses
- **Services Layer**: Contains business logic and is injected via dependency injection
- **Interface Layer**: Defines contracts for services to promote loose coupling

The project uses:
- **Dependency Injection**: Services are registered in `Program.cs` and injected into controllers
- **Async/Await**: Service methods are asynchronous for better performance
- **Logging**: Integrated logging throughout the application layers

## How to Run Locally

1. Make sure you have the .NET SDK installed.
2. In the project directory, run:
   
   ```pwsh
   dotnet run
   ```
3. Access the endpoint at [http://localhost:5000/api/name](http://localhost:5000/api/name) (or the port shown in the console).

## API Usage

The `/api/name` endpoint accepts an optional query parameter:

- Without parameter: `http://localhost:5000/api/name` returns `{"name":"DefaultUser"}`
- With parameter: `http://localhost:5000/api/name?name=John` returns `{"name":"John"}`

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
curl http://localhost:8080/api/name
curl http://localhost:8080/api/name?name=John
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

## Project Structure
- `Controllers/NameController.cs`: Contains the GET endpoint.
- `Program.cs`: Configures the web application.
- `Dockerfile`: Multi-stage Docker build configuration.
- `.dockerignore`: Excludes unnecessary files from Docker build context.
