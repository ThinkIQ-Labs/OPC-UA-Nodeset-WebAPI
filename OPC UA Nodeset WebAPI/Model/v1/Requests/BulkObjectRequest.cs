using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.Model.v1.Requests;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Requests
{
    public class BulkObjectRequest
    {
        public string ProjectId { get; set; }

        public string Uri { get; set; }

        public string ParentNodeId { get; set; }

        public List<UaObject> Types { get; set; }

        public BulkObjectRequest() { }
    }

    public class UaObject
    {
        public string ParentNodeId { get; set; }

        public string TypeDefinitionNodeId { get; set; }

        public string DisplayName { get; set; }

        public string? BrowseName { get; set; }

        public string? Description { get; set; }

        public bool? GenerateChildren { get; set; }

        public UaObject() { }
    }
}
