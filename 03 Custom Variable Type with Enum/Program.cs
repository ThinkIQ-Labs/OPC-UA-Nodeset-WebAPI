using CESMII.OpcUa.NodeSetModel.Factory.Opc;
using CESMII.OpcUa.NodeSetModel;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;

#region model prop

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

#endregion

var newNodeId = 1000;

// create enum datatype
var myEnum = new DataTypeModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    SuperType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "Enumeration"),
    DisplayName = new List<NodeModel.LocalizedText> { "ThinkIQ Work Status" },
    BrowseName = "ThinkIQ_Work_Status",
    Description = new List<NodeModel.LocalizedText> { "A small integer to capture status of item: -(0), In Work(1), Review(2), Approved(3), Done(4)." },
};

myEnum.EnumFields = new List<DataTypeModel.UaEnumField>();
myEnum.EnumFields.Add(new DataTypeModel.UaEnumField
{
    Name = "InWork",
    Value = 0
});
myEnum.EnumFields.Add(new DataTypeModel.UaEnumField
{
    Name = "UnderReview",
    Value = 1
});

myEnum.EnumFields.Add(new DataTypeModel.UaEnumField
{
    Name = "Done",
    Value = 2
});


newNodeSetModel.DataTypes.Add(myEnum);



var motorType = new ObjectTypeModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    SuperType = uaBaseModel.ObjectTypes.First(x => x.DisplayName.First().Text == "BaseObjectType"),
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

var var1 = new DataVariableModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = motorType,
    DisplayName = new List<NodeModel.LocalizedText> { "Work Status" },
    BrowseName = "Work_Status",
    Description = new List<NodeModel.LocalizedText> { "Variable for work status." },
    DataType = myEnum,
    
};

var1.Value = opcContext.JsonEncodeVariant(Int32.Parse("2"));

motorType.Properties.Add(prop1);
motorType.DataVariables.Add(var1);

newNodeSetModel.ObjectTypes.Add(motorType);

newNodeSetModel.UpdateIndices();

Console.WriteLine(newNodeSetModel.Properties.Count);

Console.WriteLine(newNodeSetModel.GetProperties().Count());

Console.WriteLine(newNodeSetModel.AllNodesByNodeId.Where(x => x.Value.GetType() == typeof(PropertyModel)).Count());


var exportedNodeSetXml = UANodeSetModelExporter.ExportNodeSetAsXml(newNodeSetModel, NodeSetModels);

File.WriteAllText("out.xml", exportedNodeSetXml);

Console.WriteLine("all nodes loaded.");

Console.ReadLine();