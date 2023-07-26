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
var workStatusEnum = new DataTypeModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    SuperType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "Enumeration"),
    DisplayName = new List<NodeModel.LocalizedText> { "ThinkIQ Work Status" },
    BrowseName = "ThinkIQ_Work_Status",
    Description = new List<NodeModel.LocalizedText> { "A small integer to capture status of item: -(0), In Work(1), Review(2), Approved(3), Done(4)." },
    EnumFields = new List<DataTypeModel.UaEnumField>
    {
        new DataTypeModel.UaEnumField
        {
            Name = "-",
            Value = 0,
            Description = new List<NodeModel.LocalizedText>(),
            DisplayName = new List<NodeModel.LocalizedText>()
        },
        new DataTypeModel.UaEnumField
        {
            Name = "Work",
            Value = 1,
            Description = new List<NodeModel.LocalizedText>(),
            DisplayName = new List<NodeModel.LocalizedText>()
        },
        new DataTypeModel.UaEnumField
        {
            Name = "Review",
            Value = 2,
            Description = new List<NodeModel.LocalizedText>(),
            DisplayName = new List<NodeModel.LocalizedText>()
        },
        new DataTypeModel.UaEnumField
        {
            Name = "Approved",
            Value = 3,
            Description = new List<NodeModel.LocalizedText>(),
            DisplayName = new List<NodeModel.LocalizedText>()
        },
        new DataTypeModel.UaEnumField
        {
            Name = "Done",
            Value = 4,
            Description = new List<NodeModel.LocalizedText>(),
            DisplayName = new List<NodeModel.LocalizedText>()
        }
    }
};


newNodeSetModel.DataTypes.Add(workStatusEnum);

var tiqTypesMetaDataVariableType = new VariableTypeModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    SuperType = uaBaseModel.VariableTypes.First(x => x.DisplayName.First().Text == "BaseDataVariableType"),
    DisplayName = new List<NodeModel.LocalizedText> { "ThinkIQ Types MetaData Type" },
    BrowseName = "ThinkIQ_Types_MetaData_Type",
    Description = new List<NodeModel.LocalizedText> { "A variable type to capture meta data for ThinkIQ type definitions." },
    Properties = new List<VariableModel>()
};

tiqTypesMetaDataVariableType.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = tiqTypesMetaDataVariableType,
    DisplayName = new List<NodeModel.LocalizedText> { "Work Status" },
    BrowseName = "Work_Status",
    Description = new List<NodeModel.LocalizedText> { "Variable for work status." },
    DataType = workStatusEnum,
    Value = null
});
tiqTypesMetaDataVariableType.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = tiqTypesMetaDataVariableType,
    DisplayName = new List<NodeModel.LocalizedText> { "Last Update" },
    BrowseName = "Last_Update",
    Description = new List<NodeModel.LocalizedText> { "Variable last update timestamp." },
    DataType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "DateTime"),
    Value = null
});

newNodeSetModel.VariableTypes.Add(tiqTypesMetaDataVariableType);

var tiqAttributesMetaDataType = new VariableTypeModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    SuperType = uaBaseModel.VariableTypes.First(x => x.DisplayName.First().Text == "BaseDataVariableType"),
    DisplayName = new List<NodeModel.LocalizedText> { "ThinkIQ Attributes MetaData Type" },
    BrowseName = "ThinkIQ_Attributes_MetaData_Type",
    Description = new List<NodeModel.LocalizedText> { "A variable type to capture meta data for ThinkIQ attributes." },
    Properties = new List<VariableModel>()
};

tiqAttributesMetaDataType.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = tiqAttributesMetaDataType,
    DisplayName = new List<NodeModel.LocalizedText> { "Work Status" },
    BrowseName = "Work_Status",
    Description = new List<NodeModel.LocalizedText> { "Variable for work status." },
    DataType = workStatusEnum,
    Value = null
});
tiqAttributesMetaDataType.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = tiqAttributesMetaDataType,
    DisplayName = new List<NodeModel.LocalizedText> { "Last Update" },
    BrowseName = "Last_Update",
    Description = new List<NodeModel.LocalizedText> { "Variable last update timestamp." },
    DataType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "DateTime"),
    Value = null
});
tiqAttributesMetaDataType.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = tiqAttributesMetaDataType,
    DisplayName = new List<NodeModel.LocalizedText> { "Hidden" },
    BrowseName = "Hidden",
    Description = new List<NodeModel.LocalizedText> { "Variable is hidden." },
    DataType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "Boolean"),
    Value = null
});

newNodeSetModel.VariableTypes.Add(tiqAttributesMetaDataType);


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

var prop1 = new DataVariableModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = motorType,
    DisplayName = new List<NodeModel.LocalizedText> { "Horse Power" },
    BrowseName = "Horse_Power",
    Description = new List<NodeModel.LocalizedText> { "Fancy Prop1" },
    DataType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "Double"),
    Properties = new List<VariableModel>(),
    DataVariables = new List<DataVariableModel>(),
    EngineeringUnit = new VariableModel.EngineeringUnitInfo
    {
        //DisplayName = "horsepower (electric)"
        DisplayName = "electric hp"
    }
};

var var2 = new DataVariableModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = prop1,
    DisplayName = new List<NodeModel.LocalizedText> { "ThinkIQ Attribute MetaData" },
    BrowseName = "ThinkIQ_Attributes_MetaData",
    Description = new List<NodeModel.LocalizedText> { "A variable type to capture meta data for ThinkIQ attributes." },
    TypeDefinition = tiqAttributesMetaDataType,
    Properties = new List<VariableModel>()
};

prop1.DataVariables.Add(var2);

var2.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = var2,
    DisplayName = new List<NodeModel.LocalizedText> { "Work Status" },
    BrowseName = "Work_Status",
    Description = new List<NodeModel.LocalizedText> { "Variable for work status." },
    DataType = workStatusEnum,
    Value = opcContext.JsonEncodeVariant(Int32.Parse("2"))

});
var2.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = var2,
    DisplayName = new List<NodeModel.LocalizedText> { "Last Update" },
    BrowseName = "Last_Update",
    Description = new List<NodeModel.LocalizedText> { "Variable last update timestamp." },
    DataType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "DateTime"),
    Value = opcContext.JsonEncodeVariant(DateTime.Parse("2023-06-25"))
});
var2.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = var2,
    DisplayName = new List<NodeModel.LocalizedText> { "Hidden" },
    BrowseName = "Hidden",
    Description = new List<NodeModel.LocalizedText> { "Variable is hidden." },
    DataType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "Boolean"),
    Value = opcContext.JsonEncodeVariant(Boolean.Parse("True"))
});

var var1 = new DataVariableModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = motorType,
    DisplayName = new List<NodeModel.LocalizedText> { "ThinkIQ Type MetaData" },
    BrowseName = "ThinkIQ_Types_MetaData",
    Description = new List<NodeModel.LocalizedText> { "A variable type to capture meta data for ThinkIQ type definitions." },
    TypeDefinition = tiqTypesMetaDataVariableType,
    Properties = new List<VariableModel>()
};

var1.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = var1,
    DisplayName = new List<NodeModel.LocalizedText> { "Work Status" },
    BrowseName = "Work_Status",
    Description = new List<NodeModel.LocalizedText> { "Variable for work status." },
    DataType = workStatusEnum,
    Value = opcContext.JsonEncodeVariant(Int32.Parse("2"))

});
var1.Properties.Add(new PropertyModel
{
    NodeSet = newNodeSetModel,
    NodeId = $"nsu={newNodeSetModel.ModelUri};i={newNodeId++}",
    Parent = var1,
    DisplayName = new List<NodeModel.LocalizedText> { "Last Update" },
    BrowseName = "Last_Update",
    Description = new List<NodeModel.LocalizedText> { "Variable last update timestamp." },
    DataType = uaBaseModel.DataTypes.First(x => x.DisplayName.First().Text == "DateTime"),
    Value = opcContext.JsonEncodeVariant(DateTime.Parse("2023-06-25"))
});

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