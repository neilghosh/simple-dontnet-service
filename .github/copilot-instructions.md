ASP.NET Core Web API project using layered architecture with dependency injection.

## Code Guidelines

- **Architecture**: Controllers → Services → Proxies layers with dependency injection
- **Interfaces**: Always separate in dedicated files (e.g., `IIpifyProxy.cs`)
- **Methods**: Use async/await, register in Program.cs with appropriate lifetimes
- **Naming**: PascalCase (classes, methods, properties), camelCase (private fields, parameters)
- **Interface naming**: Start with 'I' (e.g., INameService)
- **Code style**: Follow [Microsoft C# guidelines](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names)

## Files & Documentation

- **No .md files** unless explicitly requested by user—provide info in chat instead
- **README.md**: Update when adding/removing files, renaming classes, or changing structure
- **Architecture diagrams**: Update `arch.wsd` and PNG when PR changes components or dependencies

## Git Operations

- **Never** run `git commit` or `git push` unless the user explicitly requests it in their prompt

## After Major Changes

Build and test locally:
```bash
dotnet build simple-dotnet-service.csproj
dotnet run & PID=$!; sleep 5; curl http://localhost:8080/api/ip/outbound; kill $PID
```

For Docker build and test, use the prompt file: `.github/prompts/docker-test.prompt.md`