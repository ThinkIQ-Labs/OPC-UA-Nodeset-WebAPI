using CESMII.OpcUa.NodeSetModel;
using System.ComponentModel.DataAnnotations;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiNodeSetModel
    {
        public string ModelUri { get; set; }
        
        public string Version { get; set; }
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


        public ApiNodeSetModel() { }
        public ApiNodeSetModel(NodeSetModel aNodeSetModel) 
        {
            ModelUri= aNodeSetModel.ModelUri;
            Version= aNodeSetModel.Version;
            PublicationDate = aNodeSetModel.PublicationDate;
            DataTypesCount = aNodeSetModel.DataTypes.Count;
            DataVariablesCount=aNodeSetModel.DataVariables.Count;
            InterfacesCount= aNodeSetModel.Interfaces.Count;
            ObjectTypesCount= aNodeSetModel.ObjectTypes.Count;
            ObjectsCount= aNodeSetModel.Objects.Count;
            PropertiesCount= aNodeSetModel.Properties.Count;
            ReferenceTypesCount=aNodeSetModel.ReferenceTypes.Count;
            RequiredModelsCount=aNodeSetModel.RequiredModels.Count;
            UnknownNodesCount=aNodeSetModel.UnknownNodes.Count;
            VariableTypesCount=aNodeSetModel.VariableTypes.Count;
        }
    }
}
