// See https://aka.ms/new-console-template for more information
using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json.Linq;
using OPC_UA_Nodeset_WebAPI.Model;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Xml;

// create client
HttpClient client = new HttpClient();
client.BaseAddress = new Uri("https://localhost:7074/");
//client.BaseAddress = new Uri("https://localhost:5001/");
//client.BaseAddress = new Uri("https://opcuanodesetwebapi.azurewebsites.net/");
HttpResponseMessage response;

// get local nodesets
var localNodesets = await client.GetFromJsonAsync<Dictionary<string, ApiNodeSetInfoWithDependencies>>($"LocalNodeset");
var uaFileName = localNodesets.First(x => x.Value.ModelUri == "http://opcfoundation.org/UA/").Key;

// get session
response = await client.PutAsync($"NodesetProject/a?owner=b", null);
var sessionKey = (await response.Content.ReadFromJsonAsync<Dictionary<string, ApiNodeSetProject>>()).First().Key;

// load ua into session
response = await client.PostAsync($"NodesetProject/{sessionKey}/NodesetModel/LoadNodesetXmlFromServerAsync?uri={uaFileName}", null);
var uaModelInfo = (await response.Content.ReadFromJsonAsync<Dictionary<string, ApiNodeSetModel>>()).First();

// get ua object types
var uaObjectTypes = await client.GetFromJsonAsync<List<ApiObjectTypeModel>>($"NodesetProject/{sessionKey}/NodesetModel/{uaModelInfo.Key}/ObjectType");
var uaBaseObjectType = uaObjectTypes.First(x => x.DisplayName == "BaseObjectType");

// create new model
response = await client.PutAsJsonAsync<ApiNodeSetInfo>(
    $"NodesetProject/{sessionKey}/NodesetModel",
    new ApiNodeSetInfo
    {
        ModelUri = "http://contoso.com/UA/",
        PublicationDate = new DateTime(),
        Version = "1.0"
    });
var newModel = (await response.Content.ReadFromJsonAsync<Dictionary<string, ApiNodeSetModel>>()).First();

// add new object type
response = await client.PutAsJsonAsync<ApiNewObjectTypeModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/ObjectType",
    new ApiNewObjectTypeModel
    {
        DisplayName = "New Type",
        BrowseName = "new_type",
        Description = "fancy type",
        SuperTypeNodeId = uaBaseObjectType.NodeId
    });
var newObjectType = await response.Content.ReadFromJsonAsync<ApiObjectTypeModel>();

// add a property to the new type
response = await client.PutAsJsonAsync<ApiNewPropertyModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property",
    new ApiNewPropertyModel
    {
        DisplayName = "prop 1",
        BrowseName = "prop_1",
        Description = "fancy prop",
        ParentId = newObjectType.Id,
        DataType = "Boolean",
        Value = "False"
    });
var newProperty = await response.Content.ReadFromJsonAsync<ApiPropertyModel>();

// get xml
response = await client.GetAsync($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/GenerateXml");
var xml = await response.Content.ReadAsStringAsync();
XmlDocument xmlDoc = new XmlDocument();
xmlDoc.LoadXml(xml);
xmlDoc.Save(Console.Out);

// delete session
response = await client.DeleteAsync($"NodesetProject/{sessionKey}");
var sessionLog = await response.Content.ReadFromJsonAsync<ApiNodeSetProject>();
Console.ReadLine();