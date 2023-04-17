using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/NodesetModel/{uri}/[controller]")]
    public class PropertyController : ControllerBase
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

        public PropertyController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiPropertyModel>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
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
                var returnObject = new List<ApiPropertyModel>();
                foreach (var aProperty in activeNodesetModel.Properties)
                {
                    returnObject.Add(new ApiPropertyModel(aProperty));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiPropertyModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id, string uri, int nodeId)
        {
            var propertiesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
            {
                return propertiesListResult;
            }
            else
            {
                var propertiesList = propertiesListResult.Value as List<ApiPropertyModel>;
                var returnObject = propertiesList.FirstOrDefault(x=>x.Id== nodeId);
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
