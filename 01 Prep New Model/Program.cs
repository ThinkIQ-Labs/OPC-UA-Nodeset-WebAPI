// See https://aka.ms/new-console-template for more information
using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPC_UA_Nodeset_WebAPI.Model;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
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
        DisplayName = "Equipment",
        BrowseName = "equipment",
        Description = "Base type for all equipments",
        SuperTypeNodeId = uaBaseObjectType.NodeId
    });
var newObjectType = await response.Content.ReadFromJsonAsync<ApiObjectTypeModel>();

// add new object type for meta data
response = await client.PutAsJsonAsync<ApiNewObjectTypeModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/ObjectType",
    new ApiNewObjectTypeModel
    {
        DisplayName = "Type Meta Data",
        BrowseName = "type_meta_data",
        Description = "Meta data for all type definitions",
        SuperTypeNodeId = uaBaseObjectType.NodeId
    });
var newObjectTypeMetaData = await response.Content.ReadFromJsonAsync<ApiObjectTypeModel>();

// add a property to the meta data type
response = await client.PutAsJsonAsync<ApiNewPropertyModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property",
    new ApiNewPropertyModel
    {
        DisplayName = "Importance",
        BrowseName = "importance",
        Description = "The ranking order of attributes",
        ParentNodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace(newObjectTypeMetaData.Id, newModel.Value.ModelUri),
        DataType = "Integer",
        Value = "10"
    });
var newProperty = await response.Content.ReadFromJsonAsync<ApiPropertyModel>();

// add object to type to store metadata
// add new object type for meta data
response = await client.PutAsJsonAsync<ApiNewObjectModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Object",
    new ApiNewObjectModel
    {
        DisplayName = "Type Meta Data",
        BrowseName = "type_meta_data",
        Description = "Meta data for all type definitions",
        TypeDefinitionNodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace(newObjectTypeMetaData.Id, newModel.Value.ModelUri),
        ParentNodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace(newObjectType.Id, newModel.Value.ModelUri),
        GenerateChildren = true,
    });
var newObjectData = await response.Content.ReadFromJsonAsync<ApiObjectTypeModel>();

// get properties of the meta data object
var metaDataProperties = await client.GetFromJsonAsync<List<ApiPropertyModel>>($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property/ByParentNodeId?parentNodeId={newObjectData.NodeId}");
var aProp = metaDataProperties.First();
aProp.Value = "2";
var serializedDoc = JsonConvert.SerializeObject(aProp);
var requestContent = new StringContent(serializedDoc, Encoding.UTF8, "application/json-patch+json");
var fristProp = await client.GetFromJsonAsync<ApiPropertyModel>($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property/GetByNodeId?nodeId={metaDataProperties.First().NodeId}");
response = await client.PatchAsync($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property/PatchByNodeId?nodeId={metaDataProperties.First().NodeId}", requestContent);

// add a property to the meta data type instance
//response = await client.PutAsJsonAsync<ApiNewPropertyModel>(
//    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property",
//    new ApiNewPropertyModel
//    {
//        DisplayName = "Importance",
//        BrowseName = "importance",
//        Description = "The ranking order of attributes",
//        ParentNodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace(newObjectData.Id, newModel.Value.ModelUri),
//        DataType = "Integer",
//        Value = "3"
//    });
//var newPropertyOnInstance = await response.Content.ReadFromJsonAsync<ApiPropertyModel>();

// get xml
response = await client.GetAsync($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/GenerateXml");
var xml = await response.Content.ReadAsStringAsync();

XmlDocument xmlDoc = new XmlDocument();
xmlDoc.LoadXml(xml);
xmlDoc.Save(Console.Out);
xmlDoc.Save("out.xml");

// delete session
response = await client.DeleteAsync($"NodesetProject/{sessionKey}");
var sessionLog = await response.Content.ReadFromJsonAsync<ApiNodeSetProject>();
Console.ReadLine();