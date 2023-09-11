using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/NodesetModel/{uri}/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public ObjectController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiObjectModel>))]
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
                var returnObject = new List<ApiObjectModel>();
                foreach (var aObject in activeNodesetModel.GetObjects())
                {
                    returnObject.Add(new ApiObjectModel(aObject));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiObjectModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {

            return ApplicationInstance.GetNodeModelByNodeId(id, uri, nodeId, "ObjectModel");

            //var objectsListResult = Get(id, uri) as ObjectResult;

            //if (StatusCodes.Status200OK != objectsListResult.StatusCode)
            //{
            //    return objectsListResult;
            //}
            //else
            //{
            //    var objectsList = objectsListResult.Value as List<ApiObjectModel>;
            //    var returnObject = objectsList.FirstOrDefault(x => x.NodeId == HttpUtility.UrlDecode(nodeId));
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
        [ProducesResponseType(200, Type = typeof(List<ApiObjectModel>))]
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
                var objectsList = objectsListResult.Value as List<ApiObjectModel>;
                var returnObject = objectsList.Where(x => x.DisplayName == displayName).ToList();
                return Ok(returnObject);
            }
        }

        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ApiObjectModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PutAsync(string id, string uri, [FromBody] ApiNewObjectModel apiObjectModel)
        {

            var objectsListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != objectsListResult.StatusCode)
            {
                return objectsListResult;
            }
            else
            {
                var objects = objectsListResult.Value as List<ApiObjectModel>;
                var existingObject = objects.Where(x => x.ParentNodeId == apiObjectModel.ParentNodeId).FirstOrDefault(x => x.DisplayName == apiObjectModel.DisplayName);
                if (existingObject == null)
                {
                    // add new object
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    // look up parent object
                    var aParentModel = activeProjectInstance.NodeSetModels.FirstOrDefault(x => x.Value.ModelUri == ApiUaNodeModel.GetNameSpaceFromNodeId(apiObjectModel.ParentNodeId)).Value;
                    var parentNode = aParentModel.AllNodesByNodeId[apiObjectModel.ParentNodeId];

                    // look up type definition
                    var aObjectTypeModel = activeProjectInstance.NodeSetModels.FirstOrDefault(x => x.Value.ModelUri == ApiUaNodeModel.GetNameSpaceFromNodeId(apiObjectModel.TypeDefinitionNodeId)).Value;
                    var aObjectTypeDefinition = aObjectTypeModel.ObjectTypes.FirstOrDefault(ot => ot.NodeId == apiObjectModel.TypeDefinitionNodeId);

                    var newObjectModel = new ObjectModel
                    {
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                        Parent = parentNode,
                        TypeDefinition = aObjectTypeDefinition,
                        DisplayName = new List<NodeModel.LocalizedText> { apiObjectModel.DisplayName },
                        BrowseName = apiObjectModel.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { apiObjectModel.Description == null ? "" : apiObjectModel.Description },
                        Properties = new List<VariableModel>(),
                        DataVariables = new List<DataVariableModel>()
                    };

                    if (apiObjectModel.GenerateChildren.HasValue)
                    {
                        if (apiObjectModel.GenerateChildren.Value)
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
                    return Ok(new ApiObjectModel(newObjectModel));
                }
                else
                {
                    return BadRequest("A object with this name exists.");
                }
            }
        }


    }
}
