using CESMII.OpcUa.NodeSetModel;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.ComponentModel.DataAnnotations;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiNodeSetModel
    {
        public string? ModelUri { get; set; }
        
        public string? Version { get; set; }
        public DateTime? PublicationDate { get; set; }

        public int DataTypesCount { get; set; }
        public int DataVariablesCount { get; set; }
        public int InterfacesCount { get; set; }
        public int ObjectTypesCount { get; set; }
        public int ObjectsCount { get; set; }
        public int PropertiesCount { get; set; }
        public int ReferenceTypesCount { get; set; }
        public int RequiredModelsCount { get; set; }
        public int UnknownNodesCount { get; set; }
        public int VariableTypesCount { get; set; }

        internal NodeSetModel? NodeSetModel { get; set; }

        public ApiNodeSetModel() { }
        public ApiNodeSetModel(NodeSetModel aNodeSetModel) 
        {
            NodeSetModel = aNodeSetModel;
            ModelUri= aNodeSetModel.ModelUri;
            Version= aNodeSetModel.Version;
            PublicationDate = aNodeSetModel.PublicationDate;
            DataTypesCount = aNodeSetModel.DataTypes.Count;
            DataVariablesCount=aNodeSetModel.GetDataVariables().Count();
            InterfacesCount= aNodeSetModel.Interfaces.Count;
            ObjectTypesCount= aNodeSetModel.ObjectTypes.Count;
            ObjectsCount= aNodeSetModel.GetObjects().Count();
            PropertiesCount= aNodeSetModel.GetProperties().Count();
            ReferenceTypesCount=aNodeSetModel.ReferenceTypes.Count;
            RequiredModelsCount=aNodeSetModel.RequiredModels.Count;
            UnknownNodesCount=aNodeSetModel.UnknownNodes.Count;
            VariableTypesCount=aNodeSetModel.VariableTypes.Count;
        }
    }
}
