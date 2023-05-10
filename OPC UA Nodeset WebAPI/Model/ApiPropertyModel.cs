using CESMII.OpcUa.NodeSetModel;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiPropertyModel : ApiUaNodeModel
    {
                
        public uint ParentId { get; set; }
        internal PropertyModel? PropertyModel { get; set; }
        public ApiPropertyModel() { }

        
        public ApiPropertyModel(PropertyModel aPropertyModel) 
        {
            PropertyModel = aPropertyModel;
            NodeId = aPropertyModel.NodeId;
            DisplayName = aPropertyModel.DisplayName.First().Text;
            Description = aPropertyModel.Description.Count == 0 ? "" : aPropertyModel.Description.First().Text;
            ParentId = aPropertyModel.Parent == null ? 0 : uint.Parse(aPropertyModel.Parent.NodeId.Split("=").Last());
        }

    }
}
