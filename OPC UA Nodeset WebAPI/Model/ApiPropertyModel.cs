using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiPropertyModel : ApiUaNodeModel
    {
        public string ParentNodeId { get; set; }
        public string? DataType { get; set; }
        public string? Value { get; set; }    

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
            ParentNodeId = ParentModel == null ? "" : ParentModel.NodeId;
            DataType = aPropertyModel.DataType == null ? null : aPropertyModel.DataType.DisplayName.First().Text;

            if (aPropertyModel.Value != null)
            {
                var aPropertyModelValue = JsonConvert.DeserializeObject<JObject>(aPropertyModel.Value);
                var valueTypeId = aPropertyModelValue["Value"]["Type"].Value<int>();

                //switch (aPropertyModelValue["Value"]["Body"].Type.ToString())
                //{
                //    case "Array":
                //    case "Object":
                        Value = aPropertyModelValue["Value"]["Body"].ToString();
                //        break;
                //    default:
                //        Value = aPropertyModelValue["Value"]["Body"].Value<string>();
                //        break;
                //}

            }
            else
            {
                Value = null;
            }
        }

    }
}
