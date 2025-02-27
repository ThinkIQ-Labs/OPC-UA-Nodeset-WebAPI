using System.Text.Json;
using System.Text.Json.Serialization;

using Opc.Ua;
using Opc.Ua.Export;
using System.Reflection;
using System.Text.Json.Serialization;

namespace OPC_UA_Nodeset_WebAPI.Model.v1
{
    public class ApiNodeSetInfoWithDependencies : ApiNodeSetInfo
    {

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ApiNodeSetInfoWithDependencies> RequiredNodeSets { get; set; }

        public ApiNodeSetInfoWithDependencies()
        {

        }

        public ApiNodeSetInfoWithDependencies(UANodeSet aNodeSet)
        {
            ModelUri = aNodeSet.Models.First().ModelUri;
            Version = aNodeSet.Models.First().Version;
            PublicationDate = aNodeSet.Models.First().PublicationDate;
            RequiredNodeSets = new List<ApiNodeSetInfoWithDependencies>();
            if (aNodeSet.Models.First().RequiredModel != null)
            {
                foreach (var aRequiredNodeSet in aNodeSet.Models.First().RequiredModel)
                {
                    RequiredNodeSets.Add(new ApiNodeSetInfoWithDependencies
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
