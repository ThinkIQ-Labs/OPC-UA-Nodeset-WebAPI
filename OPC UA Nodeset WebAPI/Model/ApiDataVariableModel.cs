using CESMII.OpcUa.NodeSetModel;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiDataVariableModel : ApiUaNodeModel
    {
        public string ParentNodeId { get; set; }
        internal NodeModel? ParentModel { get; set; }
        internal DataVariableModel? DataVariableModel { get; set; }
        public ApiDataVariableModel() { }

        public ApiDataVariableModel(DataVariableModel aDataVariableModel)
        {
            DataVariableModel = aDataVariableModel;
            NodeId = aDataVariableModel.NodeId;
            DisplayName = aDataVariableModel.DisplayName.First().Text;
            Description = aDataVariableModel.Description.Count == 0 ? "" : aDataVariableModel.Description.First().Text;
            ParentModel = aDataVariableModel.Parent;
            ParentNodeId = ParentModel == null ? "" : ParentModel.NodeId;
        }

    }
}
