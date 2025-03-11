using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.api.v1.Controllers
{
    [ApiController]
    [Route("api/v1/object")]
    public class ObjectController : ControllerBase
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
            else
            {
                var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
                var returnObject = new List<ObjectModelResponse>();
                foreach (var aObject in activeNodesetModel.GetObjects())
                {
                    returnObject.Add(new ObjectModelResponse(aObject));
                }
                return Ok(returnObject);
            }
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
        public async Task<IActionResult> HttpPost([FromBody] ApiNewObjectModel request)
        {
            var id = request.ProjectId;
            var uri = request.Uri;
            var objectsListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectsListResult.StatusCode)
            {
                return objectsListResult;
            }

            var objects = objectsListResult.Value as List<ObjectModelResponse>;
            var existingObject = objects.Where(x => x.ParentNodeId == request.ParentNodeId).FirstOrDefault(x => x.DisplayName == request.DisplayName);

            if (existingObject != null)
            {
                return BadRequest("A object with this name exists.");
            }

            // add new object
            var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
            var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

            // look up parent object
            var aParentModel = activeProjectInstance.NodeSetModels.FirstOrDefault(x => x.Value.ModelUri == ApiUaNodeModel.GetNameSpaceFromNodeId(request.ParentNodeId)).Value;
            var parentNode = aParentModel.AllNodesByNodeId[request.ParentNodeId];

            // look up type definition
            var aObjectTypeModel = activeProjectInstance.NodeSetModels.FirstOrDefault(x => x.Value.ModelUri == ApiUaNodeModel.GetNameSpaceFromNodeId(request.TypeDefinitionNodeId)).Value;
            var aObjectTypeDefinition = aObjectTypeModel.ObjectTypes.FirstOrDefault(ot => ot.NodeId == request.TypeDefinitionNodeId);

            var newObjectModel = new ObjectModel
            {
                NodeSet = activeNodesetModel,
                NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                Parent = parentNode,
                TypeDefinition = aObjectTypeDefinition,
                DisplayName = new List<NodeModel.LocalizedText> { request.DisplayName },
                BrowseName = request.BrowseName,
                Description = new List<NodeModel.LocalizedText> { request.Description == null ? "" : request.Description },
                Properties = new List<VariableModel>(),
                DataVariables = new List<DataVariableModel>()
            };

            if (request.GenerateChildren.HasValue)
            {
                if (request.GenerateChildren.Value)
                {
                    aObjectTypeDefinition.Properties.ForEach(aProperty =>
                    {
                        newObjectModel.Properties.Add(new PropertyModel
                        {
                            NodeSet = activeNodesetModel,
                            NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                            Parent = newObjectModel,
                            DisplayName = aProperty.DisplayName,
                            BrowseName = aProperty.BrowseName,
                            Description = aProperty.Description,
                            DataType = aProperty.DataType,
                            Value = aProperty.Value,
                            EngineeringUnit = aProperty.EngineeringUnit,
                        });
                    });
                    aObjectTypeDefinition.DataVariables.ForEach(aDataVariable =>
                    {
                        newObjectModel.DataVariables.Add(new DataVariableModel
                        {
                            NodeSet = activeNodesetModel,
                            NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                            Parent = newObjectModel,
                            DisplayName = aDataVariable.DisplayName,
                            BrowseName = aDataVariable.BrowseName,
                            Description = aDataVariable.Description,
                            DataType = aDataVariable.DataType,
                            Value = aDataVariable.Value,
                            EngineeringUnit = aDataVariable.EngineeringUnit,
                        });
                    });
                }
            }

            activeNodesetModel.Objects.Add(newObjectModel);
            activeNodesetModel.UpdateIndices();
            return Ok(new ObjectModelResponse(newObjectModel));
        }
    }
}
