using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/NodesetModel/{uri}/[controller]")]
    public class DataVariableController : ControllerBase
    {
        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public DataVariableController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiDataVariableModel>))]
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
                var returnObject = new List<ApiDataVariableModel>();
                foreach (var aDataVariable in activeNodesetModel.DataVariables)
                {
                    returnObject.Add(new ApiDataVariableModel(aDataVariable));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiDataVariableModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id, string uri, int nodeId)
        {
            var dataVariablesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataVariablesListResult.StatusCode)
            {
                return dataVariablesListResult;
            }
            else
            {
                var dataVariablesList = dataVariablesListResult.Value as List<ApiDataVariableModel>;
                var returnObject = dataVariablesList.FirstOrDefault(x => x.Id == nodeId);
                if (returnObject != null)
                {
                    return Ok(returnObject);
                }
                else
                {
                    return NotFound("The node id does not exist.");
                }
            }
        }

        [HttpGet("ByParentId")]
        [ProducesResponseType(200, Type = typeof(List<ApiDataVariableModel>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByParentId(string id, string uri, int parentId)
        {
            var dataVariablesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataVariablesListResult.StatusCode)
            {
                return dataVariablesListResult;
            }
            else
            {
                var dataVariablesList = dataVariablesListResult.Value as List<ApiDataVariableModel>;
                var returnObject = dataVariablesList.Where(x => x.ParentId == parentId);
                if (returnObject != null)
                {
                    return Ok(returnObject);
                }
                else
                {
                    return NotFound("The node id does not exist.");
                }
            }
        }

    }
}
