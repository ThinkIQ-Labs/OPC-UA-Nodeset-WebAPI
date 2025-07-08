using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace OPC_UA_Nodeset_WebAPI.Model.v1.Requests
{
    public class ObjectRequest
    {
        [Required, MinLength(1, ErrorMessage = "ProjectId cannot be empty.")]
        public string ProjectId { get; set; }

        [Required, MinLength(1, ErrorMessage = "Uri cannot be empty.")]
        public string Uri { get; set; }

        [Required, MinLength(1, ErrorMessage = "NodeClass cannot be empty.")]
        public string ParentNodeId { get; set; }

        [Required, MinLength(1, ErrorMessage = "NodeClass cannot be empty.")]
        public string TypeDefinitionNodeId { get; set; }

        [Required, MinLength(1, ErrorMessage = "DisplayName cannot be empty.")]
        public string DisplayName { get; set; }

        [Required, MinLength(1, ErrorMessage = "BrowseName cannot be empty.")]
        public string BrowseName { get; set; }

        public string? Description { get; set; }

        public bool? GenerateChildren { get; set; }

        public ObjectRequest() { }
    }
}
