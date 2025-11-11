using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;
using FluentAssertions;

namespace API.Identity.Admin.Spa.Tests
{
    public class ConfigurationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ConfigurationControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetConfiguration_ReturnsSuccessAndCorrectContentType()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/configuration");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.ToString().Should().Contain("application/json");
        }

        [Fact]
        public async Task GetConfiguration_ReturnsValidJson()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/configuration");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            var isValidJson = IsValidJson(content);
            isValidJson.Should().BeTrue();
        }

        [Fact]
        public async Task GetConfiguration_ContainsRequiredProperties()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/configuration");
            var content = await response.Content.ReadAsStringAsync();
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            // Assert
            config.Should().NotBeNull();
            config.Should().ContainKey("hbgidentityadminspa");
            config.Should().ContainKey("hbgidentityadminspadev");
            config.Should().ContainKey("hbgidentity");
            config.Should().ContainKey("hbgidentityadminapi");
        }

        private bool IsValidJson(string content)
        {
            try
            {
                JsonDocument.Parse(content);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
