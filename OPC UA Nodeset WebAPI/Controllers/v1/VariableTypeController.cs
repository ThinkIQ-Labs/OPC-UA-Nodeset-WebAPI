using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.Model;
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
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiVariableTypeModel>))]
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
                var returnObject = new List<ApiVariableTypeModel>();
                foreach (var aVariableType in activeNodesetModel.VariableTypes)
                {
                    returnObject.Add(new ApiVariableTypeModel(aVariableType));
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
        [ProducesResponseType(200, Type = typeof(ApiVariableTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {
            return ApplicationInstance.GetNodeApiModelByNodeId(id, uri, nodeId, "VariableTypeModel");
        }

        [HttpGet("ByDisplayName/{displayName}")]
        [ProducesResponseType(200, Type = typeof(List<ApiVariableTypeModel>))]
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
                var variableTypes = variableTypesListResult.Value as List<ApiVariableTypeModel>;
                var returnObject = variableTypes.Where(x => x.DisplayName == displayName).ToList();
                return Ok(returnObject);
            }
        }


        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ApiVariableTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PutAsync(string id, string uri, [FromBody] ApiNewVariableTypeModel apiVariableTypeModel)
        {

            var variableTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != variableTypesListResult.StatusCode)
            {
                return variableTypesListResult;
            }
            else
            {
                var variableTypes = variableTypesListResult.Value as List<ApiVariableTypeModel>;
                var existingVariableType = variableTypes.FirstOrDefault(x => x.DisplayName == apiVariableTypeModel.DisplayName);
                if (existingVariableType == null)
                {
                    // add new variable type
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    var newVariableTypeModel = new VariableTypeModel
                    {
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                        SuperType = activeProjectInstance.GetNodeModelByNodeId(apiVariableTypeModel.SuperTypeNodeId) as VariableTypeModel,
                        DisplayName = new List<NodeModel.LocalizedText> { apiVariableTypeModel.DisplayName == null ? "" : apiVariableTypeModel.DisplayName },
                        BrowseName = apiVariableTypeModel.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { apiVariableTypeModel.Description == null ? "" : apiVariableTypeModel.Description },
                    };

                    activeNodesetModel.VariableTypes.Add(newVariableTypeModel);
                    activeNodesetModel.UpdateIndices();
                    return Ok(new ApiVariableTypeModel(newVariableTypeModel));
                }
                else
                {
                    return BadRequest("A variable type with this name exists.");
                }
            }
        }


    }
}
