using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CourierService.Tests
{
    public class SmockApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        [Fact]
        public async Task Get_Home_ReturnsHelloWorld()
        {
            // Arrange
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Hello World!", content);
        }
    }
}
