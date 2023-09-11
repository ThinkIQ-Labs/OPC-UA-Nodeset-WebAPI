using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/NodesetModel/{uri}/[controller]")]
    public class VariableTypeController : ControllerBase
    {
        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public VariableTypeController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiVariableTypeModel>))]
        public IActionResult Get(string id, string uri)
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
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiVariableTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {
            return ApplicationInstance.GetNodeModelByNodeId(id, uri, nodeId, "VariableTypeModel");

            //var variableTypesListResult = Get(id, uri) as ObjectResult;

            //if (StatusCodes.Status200OK != variableTypesListResult.StatusCode)
            //{
            //    return variableTypesListResult;
            //}
            //else
            //{
            //    var variableTypes = variableTypesListResult.Value as List<ApiVariableTypeModel>;
            //    var returnObject = variableTypes.FirstOrDefault(x => x.NodeId == HttpUtility.UrlDecode(nodeId));
            //    if (returnObject != null)
            //    {
            //        return Ok(returnObject);
            //    }
            //    else
            //    {
            //        return NotFound("The node id does not exist.");
            //    }
            //}
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
