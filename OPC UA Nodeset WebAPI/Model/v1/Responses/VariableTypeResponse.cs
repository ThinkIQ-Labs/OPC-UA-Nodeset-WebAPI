using CESMII.OpcUa.NodeSetModel;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Responses
{
    public class VariableTypeResponse : UaNodeResponse
    {
        public int PropertiesCount { get; set; }
        public int DataVariablesCount { get; set; }
        public int ObjectsCount { get; set; }

        public string SuperTypeNodeId { get; set; }
        public List<string> PropertiesNodeIds { get; set; }
        public List<string> DataVariablesNodeIds { get; set; }
        public List<string> ObjectsNodeIds { get; set; }
        internal VariableTypeModel? VariableTypeModel { get; set; }
        public VariableTypeResponse() { }

        public VariableTypeResponse(VariableTypeModel aVariableTypeModel)
        {
            VariableTypeModel = aVariableTypeModel;
            NodeId = aVariableTypeModel.NodeId;
            DisplayName = aVariableTypeModel.DisplayName.First().Text;
            BrowseName = aVariableTypeModel.BrowseName;
            Description = aVariableTypeModel.Description.Count == 0 ? "" : aVariableTypeModel.Description.First().Text;
            SuperTypeNodeId = aVariableTypeModel.SuperType == null ? "" : aVariableTypeModel.SuperType.NodeId;

            PropertiesCount = aVariableTypeModel.Properties == null ? 0 : aVariableTypeModel.Properties.Count;
            DataVariablesCount = aVariableTypeModel.DataVariables == null ? 0 : aVariableTypeModel.DataVariables.Count;
            ObjectsCount = aVariableTypeModel.Objects == null ? 0 : aVariableTypeModel.Objects.Count;

            PropertiesNodeIds = aVariableTypeModel.Properties == null ? new List<string>() : aVariableTypeModel.Properties.Select(x => x.NodeId).ToList();
            DataVariablesNodeIds = aVariableTypeModel.DataVariables == null ? new List<string>() : aVariableTypeModel.DataVariables.Select(x => x.NodeId).ToList();
            ObjectsNodeIds = aVariableTypeModel.Objects == null ? new List<string>() : aVariableTypeModel.Objects.Select(x => x.NodeId).ToList();
        }

    }
}
