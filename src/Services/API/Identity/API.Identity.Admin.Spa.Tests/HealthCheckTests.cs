using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using System.Net;

namespace API.Identity.Admin.Spa.Tests
{
    public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HealthCheckTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthCheck_ReturnsHealthyStatus()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Healthy");
        }

        [Fact]
        public async Task HealthCheckUI_ReturnsSuccessfulResponse()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/hc");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Identity Admin SPA Health Checks");
        }

        [Fact]
        public async Task HealthCheckUIApi_ReturnsSuccessfulResponse()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/hc-api");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
