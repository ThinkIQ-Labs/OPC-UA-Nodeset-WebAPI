using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/NodesetModel/{uri}/[controller]")]
    public class ObjectTypeController : ControllerBase
    {
        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public ObjectTypeController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiObjectTypeModel>))]
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
                var returnObject = new List<ApiObjectTypeModel>();
                foreach (var aObjectType in activeNodesetModel.ObjectTypes)
                {
                    returnObject.Add(new ApiObjectTypeModel(aObjectType));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiObjectTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id, string uri, int nodeId)
        {
            var objectTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectTypesListResult.StatusCode)
            {
                return objectTypesListResult;
            }
            else
            {
                var objectTypes = objectTypesListResult.Value as List<ApiObjectTypeModel>;
                var returnObject = objectTypes.FirstOrDefault(x => x.Id == nodeId);
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


        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ApiObjectTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PutAsync(string id, string uri, [FromBody] ApiNewObjectTypeModel apiObjectTypeModel)
        {

            var objectTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectTypesListResult.StatusCode)
            {
                return objectTypesListResult;
            }
            else
            {
                var objectTypes = objectTypesListResult.Value as List<ApiObjectTypeModel>;
                var existingObjectType = objectTypes.FirstOrDefault(x => x.DisplayName == apiObjectTypeModel.DisplayName);
                if (existingObjectType == null)
                {
                    // add new object type
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    var newObjectTypeModel = new ObjectTypeModel
                    {
                        DisplayName = new List<NodeModel.LocalizedText> { apiObjectTypeModel.DisplayName },
                        SuperType = activeProjectInstance.GetObjectTypeModelByNodeId(apiObjectTypeModel.SuperTypeNodeId),
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace(activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++, activeNodesetModel.ModelUri),
                        Description = new List<NodeModel.LocalizedText> { apiObjectTypeModel.Description == null ? "" : apiObjectTypeModel.Description },
                        Properties = new List<VariableModel>(),
                        DataVariables = new List<DataVariableModel>(),
                    };

                    activeNodesetModel.ObjectTypes.Add(newObjectTypeModel);
                    return Ok(new ApiObjectTypeModel(newObjectTypeModel));
                }
                else
                {
                    return BadRequest("An object type with this name exists.");
                }
            }
        }


    }
}
