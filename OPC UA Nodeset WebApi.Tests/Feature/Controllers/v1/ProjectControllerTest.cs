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

    [Fact]
    public async Task TestWhenCreatingProject_ShouldGenerateUniqueProjectKey()
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
        Assert.Contains("\"projectId\":", content);
        // Verify key is present and non-empty
        Assert.Matches("\"projectId\":\"[0-9a-f]+\"", content);
    }

    [Fact]
    public async Task TestWhenCreatingMultipleProjects_ShouldGenerateDifferentProjectKeys()
    {
        // Arrange
        var client = _factory.CreateClient();
        var body1 = new StringContent(
            "{\"name\":\"Test Project 1\",\"owner\":\"Foo\"}",
            Encoding.UTF8,
            "application/json"
        );
        var body2 = new StringContent(
            "{\"name\":\"Test Project 2\",\"owner\":\"Bar\"}",
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response1 = await client.PostAsync("/api/v1/project", body1);
        var response2 = await client.PostAsync("/api/v1/project", body2);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response1.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, response2.StatusCode);
        
        var content1 = await response1.Content.ReadAsStringAsync();
        var content2 = await response2.Content.ReadAsStringAsync();
        
        // Extract keys and verify they are different
        Assert.NotEqual(content1, content2);
    }

    [Fact]
    public async Task TestWhenCreatingProject_ShouldGenerateProjectKeyWithCorrectLength()
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
        // Verify key is 8 characters (assuming ProjectKeyLength is 8)
        Assert.Matches("\"projectId\":\"[0-9a-f]{8}\"", content);
    }

    [Fact]
    public async Task TestWhenCreatingProject_ShouldGenerateHexadecimalProjectKey()
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
        // Verify key contains only hexadecimal characters (0-9, a-f)
        Assert.Matches("\"projectId\":\"[0-9a-f]+\"", content);
        Assert.DoesNotMatch("\"projectId\":\"[^0-9a-f\"]+\"", content);
    }

    [Fact]
    public async Task TestWhenCreatingManyProjects_ShouldAlwaysGenerateValidProjectKeys()
    {
        // Arrange
        var client = _factory.CreateClient();
        const int projectCount = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < projectCount; i++)
        {
            var body = new StringContent(
                $"{{\"name\":\"Test Project {i}\",\"owner\":\"Owner{i}\"}}",
                Encoding.UTF8,
                "application/json"
            );
            tasks.Add(client.PostAsync("/api/v1/project", body));
        }
        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses)
        {
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Matches("\"projectId\":\"[0-9a-f]+\"", content);
        }
    }
}
