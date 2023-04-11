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

        private IActionResult ActiveNodeSetModel(string id, string uri)
        {
            var uriNoSlashes = HttpUtility.UrlDecode(uri).Replace("/", "");
            NodeSetProjectInstance aNodesetProjectInstance;
            if (ApplicationInstance.NodeSetProjectInstances.TryGetValue(id, out aNodesetProjectInstance))
            {
                NodeSetModel aNodesetModel;
                if (aNodesetProjectInstance.NodeSetModels.Keys.Select(x=>x.Replace("/","")).Contains(uriNoSlashes))
                {
                    return Ok(aNodesetProjectInstance.NodeSetModels.First(x => x.Value.ModelUri.Replace("/", "") == uriNoSlashes).Value);
                }
                else
                {
                    return NotFound("The model does not exist.");
                }
            }
            else
            {
                return NotFound("The project does not exist.");
            }
        }

        public DataVariableController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiDataVariableModel>))]
        public IActionResult Get(string id, string uri)
        {
            var activeNodesetModelResult = ActiveNodeSetModel(id, uri) as ObjectResult;

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

    }
}
