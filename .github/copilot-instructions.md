<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This is an ASP.NET Core Web API project using layered architecture with dependency injection. Use C# best practices for REST endpoints. The main GET endpoint is at /api/name and returns a name in JSON format.

## Architecture Guidelines

- Follow separation of concerns with Controllers and Services layers
- Use dependency injection for all services
- Implement interfaces for all service classes
- Use async/await patterns for service methods
- Include proper logging and error handling
- Register services in Program.cs using appropriate lifetime (Scoped, Singleton, Transient)

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

## Updating live documentation in README.md

When making changes to the API or project structure, ensure that the README.md file is updated accordingly. Update the directory structure, endpoint details, and any relevant instructions to reflect the current state of the project.