using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiDataVariableModel : ApiUaNodeModel
    {
        public string ParentNodeId { get; set; }
        public string? DataTypeNodeId { get; set; }
        public string? Value { get; set; }
        public string? TypeDefinitionNodeId { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public Dictionary<string, object> EngineeringUnit { get; set; } = new Dictionary<string, object>();
        public List<ApiNodeAndReferenceModel> AllReferencedNodes { get; set; }
        public List<ApiNodeAndReferenceModel> OtherReferencedNodes { get; set; }
        public List<ApiNodeAndReferenceModel> OtherReferencingNodes { get; set; }

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
            MinValue = aVariableModel.MinValue;
            MaxValue = aVariableModel.MaxValue;
            var engineeringUnit = aVariableModel.EngineeringUnit;

            var engineeringUnitProperties = new Dictionary<string, object>();
            if (engineeringUnit != null)
            {
                foreach (var prop in engineeringUnit.GetType().GetProperties())
                {
                    var value = prop.GetValue(engineeringUnit, null);
                    engineeringUnitProperties.Add(prop.Name, value ?? "Unknown");
                }
            }
            EngineeringUnit = engineeringUnitProperties;

            AllReferencedNodes = new List<ApiNodeAndReferenceModel>();
            if (aVariableModel.AllReferencedNodes.Count() > 0)
            {
                foreach (var aReference in aVariableModel.AllReferencedNodes)
                {
                    AllReferencedNodes.Add(new ApiNodeAndReferenceModel(aReference));
                }
            }

            OtherReferencedNodes = new List<ApiNodeAndReferenceModel>();
            foreach (var aReference in aVariableModel.OtherReferencedNodes)
            {
                OtherReferencedNodes.Add(new ApiNodeAndReferenceModel(aReference));
            }

            OtherReferencingNodes = new List<ApiNodeAndReferenceModel>();
            foreach (var aReference in aVariableModel.OtherReferencingNodes)
            {
                OtherReferencingNodes.Add(new ApiNodeAndReferenceModel(aReference));
            }

            if (aVariableModel.Value != null)
            {
                var aPropertyModelValue = JsonConvert.DeserializeObject<JObject>(aVariableModel.Value);
                var valueTypeId = aPropertyModelValue["Type"].Value<int>();

                //switch (aPropertyModelValue["Value"]["Body"].Type.ToString())
                //{
                //    case "Array":
                //    case "Object":
                Value = aPropertyModelValue["Body"].ToString();
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
