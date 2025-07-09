namespace OPC_UA_Nodeset_WebApi.Tests;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;

public class BasicTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TestWhenCreatingProjectWithValidData_ShouldReturnCreatedProject()
    {
        // Arrange
        var client = _factory.CreateClient();
        var body = new StringContent(
            "{\"name\":\"Test Project\",\"owner\":\"Foo\"}",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await client.PostAsync("/api/v1/project", body);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Project", content);
    }

    [Fact]
    public async Task TestWhenEmptyNameIsProvidedWhenCreatingProject()
    {
        // Arrange
        var client = _factory.CreateClient();
        var body = new StringContent(
            "{\"name\":\"\",\"owner\":\"Foo\"}",
            Encoding.UTF8,
            "application/json"
        );
        var response = await client.PostAsync("/api/v1/project", body);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid request: Name and Owner are required.", content);
    }

    [Fact]
    public async Task TestWhenEmptyOwnerIsProvidedWhenCreatingProject()
    {
        // Arrange
        var client = _factory.CreateClient();
        var body = new StringContent(
            "{\"name\":\"Test Project\",\"owner\":\"\"}",
            Encoding.UTF8,
            "application/json"
        );
        var response = await client.PostAsync("/api/v1/project", body);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid request: Name and Owner are required.", content);
    }

    [Fact]
    public async Task TestWhenNoNameAndOwnerIsProvidedWhenCreatingProject()
    {
        // Arrange
        var client = _factory.CreateClient();
        var body = new StringContent(
            "{\"name\":\"\",\"owner\":\"\"}",
            Encoding.UTF8,
            "application/json"
        );
        var response = await client.PostAsync("/api/v1/project", body);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid request: Name and Owner are required.", content);
    }
}
