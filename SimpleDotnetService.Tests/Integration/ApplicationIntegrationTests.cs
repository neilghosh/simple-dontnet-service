using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace SimpleDotnetService.Tests.Integration
{
    public class ApplicationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> factory;

        public ApplicationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            this.factory = factory;
        }

        [Fact]
        public async Task WeatherForecast_ReturnsSuccessStatusCode()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/weatherforecast");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task WeatherForecast_ReturnsValidJson()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/weatherforecast");
            var content = await response.Content.ReadAsStringAsync();

            var forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(forecasts);
            Assert.Equal(5, forecasts.Length);
        }

        [Fact]
        public async Task WeatherForecast_ContainsExpectedProperties()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/weatherforecast");
            var content = await response.Content.ReadAsStringAsync();

            var forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(forecasts);
            foreach (var forecast in forecasts)
            {
                Assert.True(forecast.Date != default);
                Assert.NotNull(forecast.Summary);
                Assert.InRange(forecast.TemperatureC, -20, 55);
            }
        }

        [Fact]
        public async Task WeatherForecast_CalculatesTemperatureF_Correctly()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/weatherforecast");
            var content = await response.Content.ReadAsStringAsync();

            var forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(forecasts);
            foreach (var forecast in forecasts)
            {
                var expectedF = 32 + (int)(forecast.TemperatureC / 0.5556);
                Assert.Equal(expectedF, forecast.TemperatureF);
            }
        }

        [Fact]
        public async Task Swagger_IsAvailable()
        {
            var client = factory.CreateClient();

            var response = await client.GetAsync("/swagger/index.html");

            response.EnsureSuccessStatusCode();
        }

        private class WeatherForecast
        {
            public DateOnly Date { get; set; }
            public int TemperatureC { get; set; }
            public int TemperatureF { get; set; }
            public string? Summary { get; set; }
        }
    }
}
