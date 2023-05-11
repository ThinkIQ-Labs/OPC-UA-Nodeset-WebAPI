using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiPropertyModel : ApiUaNodeModel
    {
        public uint ParentId { get; set; }
        public string? DataType { get; set; }
        public string? DefaultValue { get; set; }    

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
            DataType = aPropertyModel.DataType == null ? null : aPropertyModel.DataType.DisplayName.First().Text;

            if (aPropertyModel.Value != null)
            {
                var defaultValue = JsonConvert.DeserializeObject<JObject>(aPropertyModel.Value);
                var valueTypeId = defaultValue["Value"]["Type"].Value<int>();
                var valueBody = defaultValue["Value"]["Body"].Value<string>();

                DefaultValue = valueBody;
            }
            else
            {
                DefaultValue = null;
            }
        }

    }
}
