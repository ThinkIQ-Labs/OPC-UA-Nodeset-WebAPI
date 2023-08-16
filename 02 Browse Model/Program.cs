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

// get ua object types
var uaObjects = await client.GetFromJsonAsync<List<ApiObjectModel>>($"NodesetProject/{sessionKey}/NodesetModel/{uaModelInfo.Key}/Object");
var uaBaseObject = uaObjects.First(x => x.DisplayName == "Objects");

// get properties
var uaProperties = await client.GetFromJsonAsync<List<ApiPropertyModel>>($"NodesetProject/{sessionKey}/NodesetModel/{uaModelInfo.Key}/Property");

// get datavariables
var uaDataVariables = await client.GetFromJsonAsync<List<ApiPropertyModel>>($"NodesetProject/{sessionKey}/NodesetModel/{uaModelInfo.Key}/DataVariable");

Console.ReadLine();