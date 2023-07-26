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
        public string DataTypeNodeId { get; set; }
        public string? Value { get; set; }    

        internal NodeModel ParentModel { get; set; }
        internal PropertyModel PropertyModel { get; set; }
        public ApiPropertyModel() { }


        public ApiPropertyModel(VariableModel aVariableModel)
        {
            PropertyModel = aVariableModel as PropertyModel;
            NodeId = aVariableModel.NodeId;
            DisplayName = aVariableModel.DisplayName.First().Text;
            BrowseName = aVariableModel.BrowseName;
            Description = aVariableModel.Description.Count == 0 ? "" : aVariableModel.Description.First().Text;
            ParentModel = aVariableModel.Parent;
            ParentNodeId = ParentModel.NodeId;
            DataTypeNodeId = aVariableModel.DataType.NodeId;

            if (aVariableModel.Value != null)
            {
                var aPropertyModelValue = JsonConvert.DeserializeObject<JObject>(aVariableModel.Value);
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
