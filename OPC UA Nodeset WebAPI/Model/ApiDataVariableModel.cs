using CESMII.OpcUa.NodeSetModel;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiDataVariableModel
    {
        public string DisplayName { get; set; }
        
        public ApiDataVariableModel() { }
        
        public ApiDataVariableModel(DataVariableModel aDataVariableModel) 
        {
            DisplayName = aDataVariableModel.DisplayName.First().Text;

        }

    }
}
