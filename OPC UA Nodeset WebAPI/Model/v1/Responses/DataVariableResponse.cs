using CESMII.OpcUa.NodeSetModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Responses
{
    public class DataVariableResponse : UaNodeResponse
    {
        public string? DataTypeNodeId { get; set; }
        public string? Value { get; set; }
        public string? TypeDefinitionNodeId { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }
        public Dictionary<string, object> EngineeringUnit { get; set; } = new Dictionary<string, object>();
        public List<NodeAndReferenceResponse> AllReferencedNodes { get; set; }
        public List<NodeAndReferenceResponse> OtherReferencedNodes { get; set; }
        public List<NodeAndReferenceResponse> OtherReferencingNodes { get; set; }

        internal NodeModel? ParentModel { get; set; }
        internal DataVariableModel? DataVariableModel { get; set; }
        public DataVariableResponse() { }

        public DataVariableResponse(VariableModel aVariableModel)
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
                    var textProperty = value?.GetType().GetProperty("Text");
                    if (textProperty != null)
                    {
                        value = textProperty.GetValue(value, null);
                    }
                    engineeringUnitProperties.Add(prop.Name, value ?? "Unknown");
                }
            }
            EngineeringUnit = engineeringUnitProperties;

            AllReferencedNodes = new List<NodeAndReferenceResponse>();
            if (aVariableModel.AllReferencedNodes.Count() > 0)
            {
                foreach (var aReference in aVariableModel.AllReferencedNodes)
                {
                    AllReferencedNodes.Add(new NodeAndReferenceResponse(aReference));
                }
            }

            OtherReferencedNodes = new List<NodeAndReferenceResponse>();
            foreach (var aReference in aVariableModel.OtherReferencedNodes)
            {
                OtherReferencedNodes.Add(new NodeAndReferenceResponse(aReference));
            }

            OtherReferencingNodes = new List<NodeAndReferenceResponse>();
            foreach (var aReference in aVariableModel.OtherReferencingNodes)
            {
                OtherReferencingNodes.Add(new NodeAndReferenceResponse(aReference));
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
