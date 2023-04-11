using CESMII.OpcUa.NodeSetModel;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiPropertyModel
    {
        public string NodeId { get; set; }
        public string DisplayName { get; set; }
        
        public ApiPropertyModel() { }
        
        public ApiPropertyModel(PropertyModel aPropertyModel) 
        {
            NodeId = aPropertyModel.NodeId;
            DisplayName = aPropertyModel.DisplayName.First().Text;
        }

    }
}
