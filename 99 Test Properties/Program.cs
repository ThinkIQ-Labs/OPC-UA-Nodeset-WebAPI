using CESMII.OpcUa.NodeSetModel.Factory.Opc;
using CESMII.OpcUa.NodeSetModel;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

const string nodeSetDirectory = "NodeSets";


// get opc ua nodesets utilities online
var importer = new UANodeSetModelImporter(NullLogger.Instance);
Dictionary<string, NodeSetModel> NodeSetModels = new();
DefaultOpcUaContext opcContext = new DefaultOpcUaContext(NodeSetModels, NullLogger.Instance);

// load OPC UA
var uaUri = Path.Combine(nodeSetDirectory, "opcfoundation.org.UA.NodeSet2.xml");
var nodeSet = Opc.Ua.Export.UANodeSet.Read(new FileStream(uaUri, FileMode.Open));
await importer.LoadNodeSetModelAsync(opcContext, nodeSet);
var uaBaseModel = NodeSetModels.First().Value;

// add a new namespace that requires OPC UA
var newNodeSetModel = new NodeSetModel
{
    ModelUri = "http://whatever.net/UA/",
    RequiredModels = new List<RequiredModelInfo>
                {
                    new RequiredModelInfo { ModelUri= uaBaseModel.ModelUri, PublicationDate = uaBaseModel.PublicationDate, Version = uaBaseModel.Version}
                },
};
var newNodeId = 1000;

var motorType = new ObjectTypeModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    SuperType = uaBaseModel.ObjectTypes.First(x=>x.DisplayName.First().Text== "BaseObjectType"),
    DisplayName = new List<NodeModel.LocalizedText> { "Motor" },
    BrowseName = "Motor",
    Description = new List<NodeModel.LocalizedText> { "Fancy Motor" },
    Properties = new List<VariableModel>(),
    DataVariables = new List<DataVariableModel>(),

};

var prop1 = new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = motorType,
    DisplayName = new List<NodeModel.LocalizedText> { "Prop1" },
    BrowseName = "Prop1",
    Description = new List<NodeModel.LocalizedText> { "Fancy Prop1" },
};

motorType.Properties.Add(prop1);

newNodeSetModel.ObjectTypes.Add(motorType);

newNodeSetModel.UpdateIndices();

Console.WriteLine(newNodeSetModel.Properties.Count);

Console.WriteLine(newNodeSetModel.AllNodesByNodeId.Where(x => x.Value.GetType() == typeof(PropertyModel)).Count());


var exportedNodeSetXml = UANodeSetModelExporter.ExportNodeSetAsXml(newNodeSetModel, NodeSetModels);

File.WriteAllText("out.xml", exportedNodeSetXml);

Console.WriteLine("all nodes loaded.");

Console.ReadLine();