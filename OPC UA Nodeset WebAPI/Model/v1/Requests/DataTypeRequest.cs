using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Opc.Ua;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;
using Opc.Ua.Export.v1.Responses;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Requests
{
    public class DataTypeRequest
    {
        public string ProjectId { get; set; }

        public string Uri { get; set; }

        public string SuperTypeNodeId { get; set; }

        public string DisplayName { get; set; }

        public string? BrowseName { get; set; }

        public string? Description { get; set; }

        public List<UAEnumField> EnumFields { get; set; }

        public DataTypeRequest() { }
    }
}
