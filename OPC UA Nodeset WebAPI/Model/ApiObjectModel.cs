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
        public string TypeDefinitionNodeId { get; set; }

        internal ObjectModel? ObjectTypeModel { get; set; }
        
        public ApiObjectModel() { }

        public ApiObjectModel(ObjectModel aOjectModel)
        {
            ObjectTypeModel = aOjectModel;
            NodeId = aOjectModel.NodeId;
            DisplayName = aOjectModel.DisplayName.First().Text;
            BrowseName = aOjectModel.BrowseName;
            Description = aOjectModel.Description.Count==0 ?  "" : aOjectModel.Description.First().Text;
            TypeDefinitionNodeId = aOjectModel.TypeDefinition == null ? "" : aOjectModel.TypeDefinition.NodeId;
        }

    }
}
