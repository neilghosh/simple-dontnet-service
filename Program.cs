using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using SimpleDotnetService.Services;
using SimpleDotnetService.Services.Ip;
using SimpleDotnetService.Proxies;

var builder = WebApplication.CreateBuilder(args);

// Add Azure AD authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Simple DotNet Service API",
        Version = "v1",
        Description = "A simple .NET service with Azure AD authentication"
    });

    // Configure OAuth2 with PKCE for Swagger UI
    var azureAdConfig = builder.Configuration.GetSection("AzureAd");
    var tenantId = azureAdConfig["TenantId"];
    var clientId = azureAdConfig["ClientId"];

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { $"api://{clientId}/User.Read", "Read user information" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { $"api://{clientId}/User.Read" }
        }
    });
});
builder.Services.AddControllers();

// Register custom services
builder.Services.AddScoped<IIpAddressService, OutboundIpService>();
builder.Services.AddScoped<IIpifyProxy, IpifyProxy>();
builder.Services.AddHttpClient<IpifyProxy>();

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS for SPA client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
        options.OAuthUsePkce();
        options.OAuthScopes($"api://{builder.Configuration["AzureAd:ClientId"]}/User.Read");
    });
}

// Serve static files (for SPA)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowSPA");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();

// Make the implicit Program class accessible to tests
public partial class Program { }
