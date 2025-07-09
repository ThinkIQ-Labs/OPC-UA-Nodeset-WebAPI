using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Services;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using OPC_UA_Nodeset_WebAPI.Model.v1.Requests;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;
using System.Text.Json;

namespace OPC_UA_Nodeset_WebAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/object")]
    public class ObjectController : AbstractBaseController
    {
        private readonly ILogger<ProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public ObjectController(ILogger<ProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet("{id}/{uri}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ObjectModelResponse>))]
        public IActionResult Get(string id, string uri)
        {
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }
            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
            var returnObject = new List<ObjectModelResponse>();
            foreach (var aObject in activeNodesetModel.GetObjects())
            {
                returnObject.Add(new ObjectModelResponse(aObject));
            }
            return Ok(returnObject);
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ObjectModelResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {

            return ApplicationInstance.GetNodeApiModelByNodeId(id, uri, nodeId, "ObjectModel");
        }

        [HttpGet("ByDisplayName/{displayName}")]
        [ProducesResponseType(200, Type = typeof(List<ObjectModelResponse>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByDisplayName(string id, string uri, string displayName)
        {
            var objectsListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectsListResult.StatusCode)
            {
                return objectsListResult;
            }
            else
            {
                var objectsList = objectsListResult.Value as List<ObjectModelResponse>;
                var returnObject = objectsList.Where(x => x.DisplayName == displayName).ToList();
                return Ok(returnObject);
            }
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(ObjectModelResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> HttpPost([FromBody] ObjectRequest request)
        {
            try
            {
                var id = request.ProjectId;
                var uri = request.Uri;
                var objectsListResult = Get(id, uri) as ObjectResult;

                if (StatusCodes.Status200OK != objectsListResult.StatusCode)
                {
                    return objectsListResult;
                }

                var objects = objectsListResult.Value as List<ObjectModelResponse>;
                FindOpcType<ObjectModelResponse>(objects, request);

                var objectModelService = new ObjectModelService(ApplicationInstance);
                var newObjectModel = objectModelService.CreateObjectModel(id, uri, new UaObject
                {
                    ParentNodeId = request.ParentNodeId,
                    TypeDefinitionNodeId = request.TypeDefinitionNodeId,
                    DisplayName = request.DisplayName,
                    BrowseName = request.BrowseName,
                    Description = request.Description,
                    GenerateChildren = request.GenerateChildren
                });

                return Ok(new ObjectModelResponse(newObjectModel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new object");
                return BadRequest("Error creating new object: " + ex.Message);
            }
        }

        [HttpPost("bulk-processing")]
        [ProducesResponseType(200, Type = typeof(List<ObjectModelResponse>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> BulkProcessing([FromBody] BulkObjectRequest request)
        {
            try
            {
                var id = request.ProjectId;
                var uri = request.Uri;
                var parentNodeId = request.ParentNodeId;
                var objectsListResult = Get(id, uri) as ObjectResult;
                var objectInstancesCreated = new List<ObjectModelResponse>();

                if (StatusCodes.Status200OK != objectsListResult.StatusCode)
                {
                    throw new Exception($"Error retrieving objects for project {id} and URI {uri}");
                }

                foreach (var type in request.Types)
                {
                    var objects = objectsListResult.Value as List<ObjectModelResponse>;
                    var existingObject = objects.Where(x => x.ParentNodeId == type.ParentNodeId).FirstOrDefault(x => x.DisplayName == type.DisplayName);
                    FindOpcType<ObjectModelResponse>(objects, type);

                    var objectModelService = new ObjectModelService(ApplicationInstance);
                    var newObjectModel = objectModelService.CreateObjectModel(id, uri, type);

                    objectInstancesCreated.Add(new ObjectModelResponse(newObjectModel));
                }

                return Ok(objectInstancesCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk request");
                return BadRequest("Error processing bulk request: " + ex.Message);
            }
        }
    }
}
