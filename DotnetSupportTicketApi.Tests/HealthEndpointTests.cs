using Microsoft.AspNetCore.Mvc.Testing;

namespace DotnetSupportTicketApi.Tests
{
    public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public HealthEndpointTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetHealth_ReturnsSuccess()
        {
            var response = await _client.GetAsync("/api/health");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("API is running", content);
        }
    }
}
