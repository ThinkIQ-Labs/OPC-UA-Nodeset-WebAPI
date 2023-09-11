// See https://aka.ms/new-console-template for more information
using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json.Linq;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.Model;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Xml;

// create client
HttpClient client = new HttpClient();
//client.BaseAddress = new Uri("https://localhost:7074/");
//client.BaseAddress = new Uri("https://localhost:5001/");
client.BaseAddress = new Uri("https://opcuanodesetwebapi.azurewebsites.net/");
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

var xml = File.ReadAllText("./Nodesets/outNS-2.xml");
var xmlBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(xml));

UANodeSetBase64Upload payload = new UANodeSetBase64Upload();
payload.FileName = "out.xml";
payload.XmlBase64 = xmlBase64;

response = await client.PostAsJsonAsync<UANodeSetBase64Upload>($"NodesetProject/{sessionKey}/NodesetModel/UploadNodesetXmlFromBase64", payload);
var stuff = await response.Content.ReadAsStringAsync();
Console.WriteLine(stuff);
Console.ReadLine();