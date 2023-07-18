using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Opc.Ua;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiNewDataTypeModel
    {
        public string SuperTypeNodeId { get; set; }

        public string DisplayName { get; set; }

        public string? BrowseName { get; set; }

        public string? Description { get; set; }

        public List<UAEnumField> EnumFields { get; set; }

        public ApiNewDataTypeModel() { }


    }
}
