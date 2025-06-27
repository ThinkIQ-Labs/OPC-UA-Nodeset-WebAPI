namespace OPC_UA_Nodeset_WebApi.Tests;

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;

/**
 * to run this set of tests paste the following command in the terminal:
 *
 * dotnet test --filter "FullyQualifiedName~ObjectControllerTest"
 */
public class ObjectControllerTest : TestBase
{
    public ObjectControllerTest(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Setup()
    {
        await CreateProject();

        await LoadNodesetModel("opcfoundation.org.UA.NodeSet2.xml");
        await LoadNodesetModel("opcfoundation.org.UA.DI.NodeSet2.xml");
        await UploadXmlFromBase64("opcfoundation.org.UA.Machinery.xml");
    }

    [Fact]
    public async Task TestICreatesAnInstanceObject()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=61",
            nodeClass = "Object",
            browseName = "manager",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Manager", JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("displayName").GetString());
    }

    [Fact]
    public async Task TestItFailsToCreateAnInstanceObjectWithEmptyProjectId()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = "",
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=1",
            nodeClass = "Object",
            browseName = "manager",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TestItCreatesAnInstanceObjectWithEmptyBrowseName()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=61",
            nodeClass = "Object",
            browseName = "",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TestItCreatesBulkObjects()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object/bulk-processing", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            types = new[]
            {
                new {
                    browseName = "manager1",
                    parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
                    displayName = "Manager 1",
                    typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=61"
                },
                new {
                    browseName = "manager2",
                    parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
                    displayName = "Manager 2",
                    typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=61"
                },
                new {
                    browseName = "manager2",
                    parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
                    displayName = "Manager 2",
                    typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=61"
                },
            }
        });

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(content).RootElement;
        var firstItem = jsonResponse[0];
        var parentNodeId = firstItem.GetProperty("parentNodeId").GetString();
        var typeDefinitionNodeId = firstItem.GetProperty("typeDefinitionNodeId").GetString();
        Assert.Equal("nsu=http://opcfoundation.org/UA/;i=24", parentNodeId);
        Assert.Equal("nsu=http://opcfoundation.org/UA/;i=61", typeDefinitionNodeId);
    }

    [Fact]
    public async Task TestItFailsCreatingObjectWhenItReferencesItselfWithAnInvalidTypeDefinitionNodeId()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/Machinery/;i=1", // Invalid TypeDefinitionNodeId
            nodeClass = "Object",
            browseName = "manager",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TestItCreatesObjectWhenItReferencesItselfWithValidTypeDefinitionNodeId()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/Machinery/;i=1012", // Valid TypeDefinitionNodeId
            nodeClass = "Object",
            browseName = "manager",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TestItFailsCreatingObjectWhenItReferencesAnotherNodesetWithAnInvalidTypeDefinitionNodeId()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=1", // Invalid TypeDefinitionNodeId
            nodeClass = "Object",
            browseName = "manager",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TestItCreatesObjectWhenItReferencesAnotherNodesetWithValidTypeDefinitionNodeId()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/UA/;i=61", // Valid TypeDefinitionNodeId
            nodeClass = "Object",
            browseName = "manager",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TestItFailsCreatingObjectWhenItReferencesAnInvalidNodeset()
    {
        await Setup();

        var response = await PostAsync("/api/v1/object", new
        {
            projectId = _projectId,
            uri = "http:opcfoundation.orgUAMachinery",
            parentNodeId = "nsu=http://opcfoundation.org/UA/;i=24",
            typeDefinitionNodeId = "nsu=http://opcfoundation.org/Invalid-Nodeset/;i=1", // Invalid Nodeset
            nodeClass = "Object",
            browseName = "manager",
            displayName = "Manager",
        });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Error creating new object: Object type model for namespace http://opcfoundation.org/Invalid-Nodeset/ not found in project Test Project.", await response.Content.ReadAsStringAsync());
    }
}
