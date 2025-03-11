using CESMII.OpcUa.NodeSetModel;
using CESMII.OpcUa.NodeSetModel.Factory.Opc;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Responses
{
    public class NodeSetProjectResponse
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public Dictionary<string, string> Log { get; set; }

        public void AddToLog(string msg)
        {
            Log.Add(DateTime.UtcNow.ToString("o"), msg);
        }

        public int NodeSetModelCount { get; set; }

        public NodeSetProjectResponse() { }
        public NodeSetProjectResponse(NodeSetProjectInstance aNodesetProjectInstance)
        {
            ProjectId = aNodesetProjectInstance.ProjectId;
            Name = aNodesetProjectInstance.Name;
            Owner = aNodesetProjectInstance.Owner;
            NodeSetModelCount = aNodesetProjectInstance.NodeSetModels.Count;
            Log = aNodesetProjectInstance.Log;
        }
    }

}
