using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using OPC_UA_Nodeset_WebAPI.Model.v1.Requests;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace OPC_UA_Nodeset_WebAPI.api.v1.Controllers
{
    [ApiController]
    [Route("api/v1/variable-type")]
    public class VariableTypeController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public VariableTypeController(ILogger<ProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet("{id}/{uri}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, VariableTypeResponse>))]
        public IActionResult Get(string id, string uri, [FromQuery] Dictionary<string, string> filters = null)
        {
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }
            else
            {
                var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
                var returnObject = new List<VariableTypeResponse>();
                foreach (var aVariableType in activeNodesetModel.VariableTypes)
                {
                    returnObject.Add(new VariableTypeResponse(aVariableType));
                }

                if (filters == null)
                {
                    return Ok(returnObject);
                }

                returnObject = returnObject.Where(x =>
                    (!filters.ContainsKey("displayName") || x.DisplayName == filters["displayName"]) &&
                    (!filters.ContainsKey("browseName") || x.BrowseName == filters["browseName"]) &&
                    (!filters.ContainsKey("description") || x.Description == filters["description"]) &&
                    (!filters.ContainsKey("superTypeNodeId") || x.SuperTypeNodeId == filters["superTypeNodeId"])
                ).ToList();

                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(VariableTypeResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {
            return ApplicationInstance.GetNodeApiModelByNodeId(id, uri, nodeId, "VariableTypeModel");
        }

        [HttpGet("ByDisplayName/{displayName}")]
        [ProducesResponseType(200, Type = typeof(List<VariableTypeResponse>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByDisplayName(string id, string uri, string displayName)
        {
            var variableTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != variableTypesListResult.StatusCode)
            {
                return variableTypesListResult;
            }
            else
            {
                var variableTypes = variableTypesListResult.Value as List<VariableTypeResponse>;
                var returnObject = variableTypes.Where(x => x.DisplayName == displayName).ToList();
                return Ok(returnObject);
            }
        }


        [HttpPost]
        [ProducesResponseType(200, Type = typeof(VariableTypeResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> HttpPost([FromBody] VariableTypeRequest request)
        {
            var id = request.ProjectId;
            var uri = request.Uri;
            var variableTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != variableTypesListResult.StatusCode)
            {
                return variableTypesListResult;
            }
            var variableTypes = variableTypesListResult.Value as List<VariableTypeResponse>;
            var existingVariableType = variableTypes.FirstOrDefault(x => x.DisplayName == request.DisplayName);

            if (existingVariableType != null)
            {
                return BadRequest("A variable type with this name exists.");
            }

            // add new variable type
            var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
            var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

            var newVariableTypeModel = new VariableTypeModel
            {
                NodeSet = activeNodesetModel,
                NodeId = UaNodeResponse.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                SuperType = activeProjectInstance.GetNodeModelByNodeId(request.SuperTypeNodeId) as VariableTypeModel,
                DisplayName = new List<NodeModel.LocalizedText> { request.DisplayName == null ? "" : request.DisplayName },
                BrowseName = request.BrowseName,
                Description = new List<NodeModel.LocalizedText> { request.Description == null ? "" : request.Description },
            };

            activeNodesetModel.VariableTypes.Add(newVariableTypeModel);
            activeNodesetModel.UpdateIndices();
            return Ok(new VariableTypeResponse(newVariableTypeModel));
        }
    }
}
