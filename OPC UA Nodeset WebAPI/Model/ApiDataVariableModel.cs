using CESMII.OpcUa.NodeSetModel;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiDataVariableModel : ApiUaNodeModel
    {
        public string ParentNodeId { get; set; }
        public string? DataTypeNodeId { get; set; }

        public string? TypeDefinitionNodeId { get; set; }

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

        }

    }
}
