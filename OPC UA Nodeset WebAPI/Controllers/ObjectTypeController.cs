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
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {
            var objectTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectTypesListResult.StatusCode)
            {
                return objectTypesListResult;
            }
            else
            {
                var objectTypes = objectTypesListResult.Value as List<ApiObjectTypeModel>;
                var returnObject = objectTypes.FirstOrDefault(x => x.NodeId == HttpUtility.UrlDecode(nodeId));
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

        [HttpGet("ByDisplayName/{displayName}")]
        [ProducesResponseType(200, Type = typeof(List<ApiObjectTypeModel>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByDisplayName(string id, string uri, string displayName)
        {
            var objectTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectTypesListResult.StatusCode)
            {
                return objectTypesListResult;
            }
            else
            {
                var objectTypes = objectTypesListResult.Value as List<ApiObjectTypeModel>;
                var returnObject = objectTypes.Where(x => x.DisplayName == displayName).ToList();
                return Ok(returnObject);
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
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                        SuperType = activeProjectInstance.GetNodeModelByNodeId(apiObjectTypeModel.SuperTypeNodeId) as ObjectTypeModel,
                        DisplayName = new List<NodeModel.LocalizedText> { apiObjectTypeModel.DisplayName },
                        BrowseName = apiObjectTypeModel.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { apiObjectTypeModel.Description == null ? "" : apiObjectTypeModel.Description },
                        Properties = new List<VariableModel>(),
                        DataVariables = new List<DataVariableModel>(),
                    };

                    activeNodesetModel.ObjectTypes.Add(newObjectTypeModel);
                    activeNodesetModel.UpdateIndices();
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
