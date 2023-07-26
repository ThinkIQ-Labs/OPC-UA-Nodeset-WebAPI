using CESMII.OpcUa.NodeSetModel;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiVariableTypeModel : ApiUaNodeModel
    {
        public string SuperTypeNodeId { get; set; }
        public List<ApiPropertyModel> Properties { get; set; }
        public List<ApiDataVariableModel> DataVariables { get; set; }
        internal VariableTypeModel? VariableTypeModel { get; set; }
        public ApiVariableTypeModel() { }

        public ApiVariableTypeModel(VariableTypeModel aVariableTypeModel)
        {
            VariableTypeModel = aVariableTypeModel;
            NodeId = aVariableTypeModel.NodeId;
            DisplayName = aVariableTypeModel.DisplayName.First().Text;
            BrowseName = aVariableTypeModel.BrowseName;
            Description = aVariableTypeModel.Description.Count == 0 ? "" : aVariableTypeModel.Description.First().Text;
            SuperTypeNodeId = aVariableTypeModel.SuperType == null ? "" : aVariableTypeModel.SuperType.NodeId;
            Properties = aVariableTypeModel.Properties.Select(x => new ApiPropertyModel(x)).ToList();
            DataVariables = aVariableTypeModel.DataVariables.Select(x => new ApiDataVariableModel(x)).ToList();
        }

    }
}
