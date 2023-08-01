using CESMII.OpcUa.NodeSetModel;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiDataVariableModel : ApiUaNodeModel
    {
        public string ParentNodeId { get; set; }
        public string? DataTypeNodeId { get; set; }

        public string? TypeDefinitionNodeId { get; set; }

        internal NodeModel? ParentModel { get; set; }
        internal DataVariableModel? DataVariableModel { get; set; }
        public ApiDataVariableModel() { }

        public ApiDataVariableModel(VariableModel aVariableModel)
        {
            DataVariableModel = aVariableModel as DataVariableModel;
            NodeId = aVariableModel.NodeId;
            DisplayName = aVariableModel.DisplayName.First().Text;
            BrowseName = aVariableModel.BrowseName;
            Description = aVariableModel.Description.Count == 0 ? "" : aVariableModel.Description.First().Text;
            ParentModel = aVariableModel.Parent;
            ParentNodeId = ParentModel == null ? "" : ParentModel.NodeId;
            DataTypeNodeId = aVariableModel.DataType == null ? "" : aVariableModel.DataType.NodeId;
            TypeDefinitionNodeId = aVariableModel.TypeDefinition == null ? "" : aVariableModel.TypeDefinition.NodeId;
        }

    }
}
