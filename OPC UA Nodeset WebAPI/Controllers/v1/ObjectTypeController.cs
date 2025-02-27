using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Opc.Ua;
using OPC_UA_Nodeset_WebAPI.Model.v1;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace OPC_UA_Nodeset_WebAPI.api.v1.Controllers
{
    [ApiController]
    [Route("api/v1/object-type")]
    public class ObjectTypeController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public ObjectTypeController(ILogger<ProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet("{id}/{uri}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiObjectTypeModel>))]
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
                var returnObject = new List<ApiObjectTypeModel>();
                foreach (var aObjectType in activeNodesetModel.ObjectTypes)
                {
                    returnObject.Add(new ApiObjectTypeModel(aObjectType));
                }

                if (filters == null)
                {
                    return Ok(returnObject);
                }

                returnObject = returnObject.Where(x =>
                    (!filters.ContainsKey("displayName") || x.DisplayName == filters["displayName"]) &&
                    (!filters.ContainsKey("browseName") || x.BrowseName == filters["browseName"]) &&
                    (!filters.ContainsKey("description") || x.Description == filters["description"]) &&
                    (!filters.ContainsKey("superTypeNodeId") || x.SuperTypeNodeId == filters["superTypeNodeId"]) &&
                    (!filters.ContainsKey("propertiesCount") || x.PropertiesCount.ToString() == filters["propertiesCount"]) &&
                    (!filters.ContainsKey("dataVariablesCount") || x.DataVariablesCount.ToString() == filters["dataVariablesCount"]) &&
                    (!filters.ContainsKey("objectsCount") || x.ObjectsCount.ToString() == filters["objectsCount"]) &&
                    (!filters.ContainsKey("propertiesNodeIds") || x.PropertiesNodeIds.Contains(filters["propertiesNodeIds"])) &&
                    (!filters.ContainsKey("dataVariablesNodeIds") || x.DataVariablesNodeIds.Contains(filters["dataVariablesNodeIds"])) &&
                    (!filters.ContainsKey("objectsNodeIds") || x.ObjectsNodeIds.Contains(filters["objectsNodeIds"]))
                ).ToList();

                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiObjectTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {

            return ApplicationInstance.GetNodeApiModelByNodeId(id, uri, nodeId, "ObjectTypeModel");
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

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(ApiObjectTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> HttpPost([FromBody] ApiNewObjectTypeModel request)
        {
            var id = request.ProjectId;
            var uri = request.Uri;
            var objectTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectTypesListResult.StatusCode)
            {
                return objectTypesListResult;
            }

            var objectTypes = objectTypesListResult.Value as List<ApiObjectTypeModel>;
            var existingObjectType = objectTypes.FirstOrDefault(x => x.DisplayName == request.DisplayName);

            if (existingObjectType != null)
            {
                return BadRequest("An object type with this name exists.");
            }

            // add new object type
            var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
            var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

            var newObjectTypeModel = new ObjectTypeModel
            {
                NodeSet = activeNodesetModel,
                NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                SuperType = activeProjectInstance.GetNodeModelByNodeId(request.SuperTypeNodeId) as ObjectTypeModel,
                DisplayName = new List<NodeModel.LocalizedText> { request.DisplayName },
                BrowseName = request.BrowseName,
                Description = new List<NodeModel.LocalizedText> { request.Description == null ? "" : request.Description },
                Properties = new List<VariableModel>(),
                DataVariables = new List<DataVariableModel>(),
            };

            activeNodesetModel.ObjectTypes.Add(newObjectTypeModel);
            activeNodesetModel.UpdateIndices();
            return Ok(new ApiObjectTypeModel(newObjectTypeModel));
        }


    }
}
