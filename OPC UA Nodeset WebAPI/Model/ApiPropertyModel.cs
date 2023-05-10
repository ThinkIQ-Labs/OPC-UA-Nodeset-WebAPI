using CESMII.OpcUa.NodeSetModel;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiPropertyModel : ApiUaNodeModel
    {
        public uint ParentId { get; set; }
        internal NodeModel? ParentModel { get; set; }
        internal PropertyModel? PropertyModel { get; set; }
        public ApiPropertyModel() { }


        public ApiPropertyModel(PropertyModel aPropertyModel)
        {
            PropertyModel = aPropertyModel;
            NodeId = aPropertyModel.NodeId;
            DisplayName = aPropertyModel.DisplayName.First().Text;
            BrowseName = aPropertyModel.BrowseName;
            Description = aPropertyModel.Description.Count == 0 ? "" : aPropertyModel.Description.First().Text;
            ParentModel = aPropertyModel.Parent;
            ParentId = ParentModel == null ? 0 : ApiUaNodeModel.GetIdFromNodeId(ParentModel.NodeId);
        }

    }
}
