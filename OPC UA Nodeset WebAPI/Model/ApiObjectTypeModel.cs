using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiObjectTypeModel : ApiUaNodeModel
    {
        public int PropertiesCount { get; set; }
        public int DataVariablesCount { get; set; }
        public int ObjectsCount { get; set; }

        public string SuperTypeNodeId { get; set; }

        public List<string> PropertiesNodeIds { get; set; }
        public List<string> DataVariablesNodeIds { get; set; }
        public List<string> ObjectsNodeIds { get; set; }

        internal ObjectTypeModel? ObjectTypeModel { get; set; }
        
        public ApiObjectTypeModel() { }

        public ApiObjectTypeModel(ObjectTypeModel aOjectTypeModel)
        {
            ObjectTypeModel = aOjectTypeModel;
            NodeId = aOjectTypeModel.NodeId;
            DisplayName = aOjectTypeModel.DisplayName.First().Text;
            BrowseName = aOjectTypeModel.BrowseName;
            Description = aOjectTypeModel.Description.Count==0 ?  "" : aOjectTypeModel.Description.First().Text;
            SuperTypeNodeId = aOjectTypeModel.SuperType == null ? "" : aOjectTypeModel.SuperType.NodeId;
            
            PropertiesCount = aOjectTypeModel.Properties == null ? 0 : aOjectTypeModel.Properties.Count;
            DataVariablesCount = aOjectTypeModel.DataVariables == null ? 0 : aOjectTypeModel.DataVariables.Count;
            ObjectsCount = aOjectTypeModel.Objects == null ? 0 : aOjectTypeModel.Objects.Count;
            
            PropertiesNodeIds = aOjectTypeModel.Properties == null ? new List<string>() : aOjectTypeModel.Properties.Select(x=>x.NodeId).ToList();
            DataVariablesNodeIds = aOjectTypeModel.DataVariables == null ? new List<string>() : aOjectTypeModel.DataVariables.Select(x=>x.NodeId).ToList();
            ObjectsNodeIds = aOjectTypeModel.Objects == null ? new List<string>() : aOjectTypeModel.Objects.Select(x=>x.NodeId).ToList();
        }

    }
}
