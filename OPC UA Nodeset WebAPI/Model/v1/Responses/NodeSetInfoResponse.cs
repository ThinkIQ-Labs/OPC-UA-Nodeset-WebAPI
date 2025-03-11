using System.Text.Json;
using System.Text.Json.Serialization;

using Opc.Ua;
using Opc.Ua.Export;
using System.Reflection;
using System.Text.Json.Serialization;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Responses
{
    public class NodeSetInfoResponse
    {
        public string? ModelUri { get; set; }

        public string? Version { get; set; }
        public DateTime? PublicationDate { get; set; }

        public NodeSetInfoResponse()
        {

        }

        public NodeSetInfoResponse(UANodeSet aNodeSet)
        {
            ModelUri = aNodeSet.Models.First().ModelUri;
            Version = aNodeSet.Models.First().Version;
            PublicationDate = aNodeSet.Models.First().PublicationDate;
        }

    }
}
