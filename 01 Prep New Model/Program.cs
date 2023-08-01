// See https://aka.ms/new-console-template for more information
using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.Model;
using System.Net;
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

// get ua data types
var uaDataTypes = await client.GetFromJsonAsync<List<ApiObjectTypeModel>>($"NodesetProject/{sessionKey}/NodesetModel/{uaModelInfo.Key}/DataType");
var uaEnumDataType = uaDataTypes.First(x => x.DisplayName == "Enumeration");

// get ua variable types
var uaVariableTypes = await client.GetFromJsonAsync<List<ApiVariableTypeModel>>($"NodesetProject/{sessionKey}/NodesetModel/{uaModelInfo.Key}/VariableType");
var uaBaseDataVariableType = uaVariableTypes.First(x => x.DisplayName == "BaseDataVariableType");

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

// add a new enum data type
response = await client.PutAsJsonAsync<ApiNewDataTypeModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/DataType", 
    new ApiNewDataTypeModel
    {
        SuperTypeNodeId = uaEnumDataType.NodeId,
        DisplayName= "ThinkIQ Work Status",
        BrowseName= "ThinkIQ_Work_Status",
        Description= "A small integer to capture status of item: -(0), In Work(1), Review(2), Approved(3), Done(4).",
        EnumFields=new List<UAEnumField>
        {
            new UAEnumField
            {
                Name = "-",
                Value = 0
            },
            new UAEnumField
            {
                Name = "Work",
                Value = 1
            },
            new UAEnumField
            {
                Name = "Review",
                Value = 2
            },
            new UAEnumField
            {
                Name = "Approved",
                Value = 3
            },
            new UAEnumField
            {
                Name = "Done",
                Value = 4
            }
        },
    });
var newDataType = await response.Content.ReadFromJsonAsync<ApiDataTypeModel>();

// get data types
var newDataTypes = await client.GetFromJsonAsync<List<ApiDataTypeModel>>($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/DataType");

// add new variable type
response = await client.PutAsJsonAsync<ApiNewVariableTypeModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/VariableType",
    new ApiNewVariableTypeModel
    {
        SuperTypeNodeId = uaBaseDataVariableType.NodeId,
        DisplayName = "ThinkIQ Types MetaData Type",
        BrowseName = "ThinkIQ_Types_MetaData_Type",
        Description = "A variable type to capture meta data for ThinkIQ type definitions.",
    });
var newVariableType = await response.Content.ReadFromJsonAsync<ApiVariableTypeModel>();

// get variable types
var newVariableTypes = await client.GetFromJsonAsync<List<ApiVariableTypeModel>>($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/VariableType");

// add work status property to new variable type
response = await client.PutAsJsonAsync<ApiNewPropertyModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property",
    new ApiNewPropertyModel
    {
        ParentNodeId = newVariableType.NodeId,
        DisplayName = "Work Status",
        BrowseName = "Work_Status",
        Description = "Variable for work status.",
        DataTypeNodeId = newDataType.NodeId,
        Value = null
    });
var newProperty = await response.Content.ReadFromJsonAsync<ApiPropertyModel>();

// add importance property to the meta data type
response = await client.PutAsJsonAsync<ApiNewPropertyModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/Property",
    new ApiNewPropertyModel
    {
        ParentNodeId = newVariableType.NodeId,
        DisplayName = "Importance",
        BrowseName = "importance",
        Description = "The ranking order of attributes",
        DataTypeNodeId = uaDataTypes.First(x => x.DisplayName == "Integer").NodeId,
        Value = "10"
    });
newProperty = await response.Content.ReadFromJsonAsync<ApiPropertyModel>();

// get variable types
newVariableTypes = await client.GetFromJsonAsync<List<ApiVariableTypeModel>>($"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/VariableType");

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

// add the variable to the equipment
// add work status property to new variable type
response = await client.PutAsJsonAsync<ApiNewDataVariableModel>(
    $"NodesetProject/{sessionKey}/NodesetModel/{newModel.Key}/DataVariable",
    new ApiNewDataVariableModel
    {
        ParentNodeId = newObjectType.NodeId,
        DisplayName = "ThinkIQ MetaData",
        BrowseName = "ThinkIQ_MetaData",
        Description = "Variable for ThinkIQ MetaData.",
        TypeDefinitionNodeId = newVariableType.NodeId,
        Value = null,
        GenerateChildren = true
    });
var newDataVariable = await response.Content.ReadFromJsonAsync<ApiDataVariableModel>();


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