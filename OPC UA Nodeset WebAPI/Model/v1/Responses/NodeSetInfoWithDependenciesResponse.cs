using System.Text.Json;
using System.Text.Json.Serialization;

using Opc.Ua;
using Opc.Ua.Export;
using System.Reflection;
using System.Text.Json.Serialization;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Responses
{
    public class NodeSetInfoWithDependenciesResponse : NodeSetInfoResponse
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<NodeSetInfoWithDependenciesResponse> RequiredNodeSets { get; set; }

        public NodeSetInfoWithDependenciesResponse()
        {

        }

        public NodeSetInfoWithDependenciesResponse(UANodeSet aNodeSet)
        {
            ModelUri = aNodeSet.Models.First().ModelUri;
            Version = aNodeSet.Models.First().Version;
            PublicationDate = aNodeSet.Models.First().PublicationDate;
            RequiredNodeSets = new List<NodeSetInfoWithDependenciesResponse>();
            if (aNodeSet.Models.First().RequiredModel != null)
            {
                foreach (var aRequiredNodeSet in aNodeSet.Models.First().RequiredModel)
                {
                    RequiredNodeSets.Add(new NodeSetInfoWithDependenciesResponse
                    {
                        ModelUri = aRequiredNodeSet.ModelUri,
                        Version = aRequiredNodeSet.Version,
                        PublicationDate = aRequiredNodeSet.PublicationDate
                    });
                }
            }
        }

    }
}
