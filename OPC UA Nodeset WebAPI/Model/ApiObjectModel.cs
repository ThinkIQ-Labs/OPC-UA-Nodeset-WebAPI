using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiObjectModel : ApiUaNodeModel
    {
        public string ParentNodeId { get; set; }

        public string TypeDefinitionNodeId { get; set; }

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
        }

    }
}
