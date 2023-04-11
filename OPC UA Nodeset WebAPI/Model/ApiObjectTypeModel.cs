using CESMII.OpcUa.NodeSetModel;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiObjectTypeModel
    {
        public string DisplayName { get; set; }
        public int PropertiesCount { get; set; }
        public int DataVariablesCount { get; set; }
        
        public ApiObjectTypeModel() { }
        
        public ApiObjectTypeModel(ObjectTypeModel aOjectTypeModel) 
        {
            DisplayName = aOjectTypeModel.DisplayName.First().Text;
            PropertiesCount = aOjectTypeModel.Properties.Count;
            DataVariablesCount = aOjectTypeModel.DataVariables.Count;

        }

    }
}
