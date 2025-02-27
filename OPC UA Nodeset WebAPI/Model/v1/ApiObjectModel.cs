using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model.v1
{
    public class ApiObjectModel : ApiUaNodeModel
    {
        public string ParentNodeId { get; set; }

        public string TypeDefinitionNodeId { get; set; }

        public List<ApiNodeAndReferenceModel> AllReferencedNodes { get; set; }
        public List<ApiNodeAndReferenceModel> OtherReferencedNodes { get; set; }
        public List<ApiNodeAndReferenceModel> OtherReferencingNodes { get; set; }

        internal ObjectModel? ObjectModel { get; set; }
        internal ObjectTypeModel? TypeDefinition { get; set; }
        internal NodeModel? ParentNodeModel { get; set; }

        public ApiObjectModel() { }

        public ApiObjectModel(ObjectModel aObjectModel)
        {
            ObjectModel = aObjectModel;
            NodeId = aObjectModel.NodeId;
            DisplayName = aObjectModel.DisplayName.First().Text;
            BrowseName = aObjectModel.BrowseName;
            Description = aObjectModel.Description.Count == 0 ? "" : aObjectModel.Description.First().Text;

            TypeDefinition = aObjectModel.TypeDefinition == null ? null : aObjectModel.TypeDefinition;
            TypeDefinitionNodeId = aObjectModel.TypeDefinition == null ? "" : aObjectModel.TypeDefinition.NodeId;

            ParentNodeModel = aObjectModel.Parent == null ? null : aObjectModel.Parent;
            ParentNodeId = aObjectModel.Parent == null ? "" : aObjectModel.Parent.NodeId;

            AllReferencedNodes = new List<ApiNodeAndReferenceModel>();
            if (aObjectModel.AllReferencedNodes.Count() > 0)
            {
                foreach (var aReference in aObjectModel.AllReferencedNodes)
                {
                    AllReferencedNodes.Add(new ApiNodeAndReferenceModel(aReference));
                }
            }

            OtherReferencedNodes = new List<ApiNodeAndReferenceModel>();
            foreach (var aReference in aObjectModel.OtherReferencedNodes)
            {
                OtherReferencedNodes.Add(new ApiNodeAndReferenceModel(aReference));
            }

            OtherReferencingNodes = new List<ApiNodeAndReferenceModel>();
            foreach (var aReference in aObjectModel.OtherReferencingNodes)
            {
                OtherReferencingNodes.Add(new ApiNodeAndReferenceModel(aReference));
            }
        }

    }
}
