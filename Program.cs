using SimpleDotnetService.Services;
using SimpleDotnetService.Services.Ip;
using SimpleDotnetService.Proxies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Register custom services
builder.Services.AddScoped<IIpAddressService, OutboundIpService>();
builder.Services.AddScoped<IIpifyProxy, IpifyProxy>();
builder.Services.AddHttpClient<IpifyProxy>();

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();

// Make the implicit Program class accessible to tests
public partial class Program { }
