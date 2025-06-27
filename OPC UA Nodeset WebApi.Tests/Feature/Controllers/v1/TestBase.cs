namespace OPC_UA_Nodeset_WebApi.Tests;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;

    protected HttpClient _client = null!;

    protected string _projectId = string.Empty;

    public TestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    protected async Task CreateProject()
    {
        var response = await PostAsync("/api/v1/project", new
        {
            name = "Test Project",
            owner = "Foo"
        });
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        _projectId = JsonDocument.Parse(content).RootElement.GetProperty("projectId").GetString();
    }

    protected async Task<HttpResponseMessage> LoadNodesetModel(string uri)
    {
        return await PostAsync("/api/v1/nodeset-model/load-xml-from-server-async", new
        {
            projectId = _projectId,
            uri = uri
        });
    }

    public void EnsureNodeSetsDirectoryExists()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "NodeSets");
        if (!Directory.Exists(path))
        {

            Directory.CreateDirectory(path);
        }
    }

    public async Task<HttpResponseMessage> UploadXmlFromBase64(string xmlFileName)
    {
        EnsureNodeSetsDirectoryExists();
        var xmlPath = Path.Combine(AppContext.BaseDirectory, "TestData", xmlFileName);
        var fileBytes = await File.ReadAllBytesAsync(xmlPath);
        var base64Xml = Convert.ToBase64String(fileBytes);
        return await PostAsync("/api/v1/nodeset-model/upload-xml-from-base-64", new
        {
            projectId = _projectId,
            xmlBase64 = base64Xml
        });
    }

    private StringContent CreateJsonContent(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody);
        return new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );
    }

    public async Task<HttpResponseMessage> PostAsync(string url, object requestBody)
    {
        var body = CreateJsonContent(requestBody);
        return await _client.PostAsync(url, body);
    }
}
