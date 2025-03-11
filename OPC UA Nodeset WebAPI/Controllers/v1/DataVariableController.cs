using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using OPC_UA_Nodeset_WebAPI.Model.v1.Requests;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;

namespace OPC_UA_Nodeset_WebAPI.api.v1.Controllers
{
    [ApiController]
    [Route("api/v1/data-variable")]
    public class DataVariableController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public DataVariableController(ILogger<ProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet("{id}/{uri}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, DataVariableResponse>))]
        public IActionResult Get(string id, string uri)
        {
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }
            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
            var returnObject = new List<DataVariableResponse>();
            foreach (var aDataVariable in activeNodesetModel.GetDataVariables())
            {
                returnObject.Add(new DataVariableResponse(aDataVariable));
            }
            return Ok(returnObject);
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(DataVariableResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id, string uri, string nodeId)
        {
            return ApplicationInstance.GetNodeApiModelByNodeId(id, uri, nodeId, "DataVariableModel");
        }

        [HttpGet("ByParentNodeId")]
        [ProducesResponseType(200, Type = typeof(List<DataVariableResponse>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByParentNodeId(string id, string uri, string parentNodeId)
        {
            var aParentNodeResult = ApplicationInstance.GetNodeModelByNodeId(id, uri, parentNodeId) as ObjectResult;

            if (StatusCodes.Status200OK != aParentNodeResult.StatusCode)
            {
                return aParentNodeResult;
            }
            NodeModel aNodeModel = aParentNodeResult.Value as NodeModel;

            List<DataVariableModel> dataVariables = new List<DataVariableModel>();
            switch (aNodeModel)
            {
                case ObjectTypeModel aObjectTypeModel:
                    dataVariables = aObjectTypeModel.DataVariables;
                    break;
                case ObjectModel aObjectModel:
                    dataVariables = aObjectModel.DataVariables;
                    break;
                case DataVariableModel aDataVariableModel:
                    dataVariables = aDataVariableModel.DataVariables;
                    break;
                default:
                    break;
            }

            var returnObject = dataVariables.Select(x => new DataVariableResponse(x));

            return Ok(returnObject);
        }

        [HttpPost]
        [ProducesResponseType(200, Type = typeof(DataVariableResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult Post([FromBody] DataVariableRequest request)
        {
            var id = request.ProjectId;
            var uri = request.Uri;
            var dataVariablesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataVariablesListResult.StatusCode)
            {
                return dataVariablesListResult;
            }
            var dataVariablesList = dataVariablesListResult.Value as List<DataVariableResponse>;
            var existingDataVariable = dataVariablesList.Where(x => x.ParentNodeId == request.ParentNodeId).FirstOrDefault(x => x.DisplayName == request.DisplayName);
            if (existingDataVariable == null)
            {
                // add new dataVariable
                var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                // look up parent object
                var parentNode = activeProjectInstance.GetNodeModelByNodeId(request.ParentNodeId);

                // look up data type
                DataTypeModel aDataType = request.DataTypeNodeId == null ? null : activeProjectInstance.GetNodeModelByNodeId(request.DataTypeNodeId) as DataTypeModel;

                VariableTypeModel aTypeDefinition = request.TypeDefinitionNodeId == null ? null : activeProjectInstance.GetNodeModelByNodeId(request.TypeDefinitionNodeId) as VariableTypeModel;

                var newDataVariableModel = new DataVariableModel
                {
                    NodeSet = activeNodesetModel,
                    NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                    Parent = parentNode,
                    DisplayName = new List<NodeModel.LocalizedText> { request.DisplayName },
                    BrowseName = request.BrowseName,
                    Description = new List<NodeModel.LocalizedText> { request.Description == null ? "" : request.Description },
                    DataType = aDataType,
                    TypeDefinition = aTypeDefinition
                };

                if (request.GenerateChildren.HasValue)
                {
                    if (request.GenerateChildren.Value)
                    {
                        aTypeDefinition.Properties.ForEach(aProperty =>
                        {
                            newDataVariableModel.Properties.Add(new PropertyModel
                            {
                                NodeSet = activeNodesetModel,
                                NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                                Parent = newDataVariableModel,
                                DisplayName = aProperty.DisplayName,
                                BrowseName = aProperty.BrowseName,
                                Description = aProperty.Description,
                                DataType = aProperty.DataType,
                                Value = aProperty.Value,
                                EngineeringUnit = aProperty.EngineeringUnit,
                            });
                        });
                        aTypeDefinition.DataVariables.ForEach(aDataVariable =>
                        {
                            newDataVariableModel.DataVariables.Add(new DataVariableModel
                            {
                                NodeSet = activeNodesetModel,
                                NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                                Parent = newDataVariableModel,
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

                // add value
                if (request.Value != null)
                {
                    switch (aDataType.DisplayName.First().Text)
                    {
                        case "Integer":
                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "SByte":
                            //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                            int aIntValue;
                            if (Int32.TryParse(request.Value, out aIntValue))
                            {
                                newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aIntValue).Json;
                            }
                            break;
                        case "Boolean":
                        case "Bool":
                            //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Boolean");
                            Boolean aBoolValue;
                            if (Boolean.TryParse(request.Value, out aBoolValue))
                            {
                                newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aBoolValue).Json;
                            }
                            break;
                        case "DateTime":
                        case "UtcTime":
                            //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "DateTime");
                            DateTime aDateTimeValue;
                            if (DateTime.TryParse(request.Value, out aDateTimeValue))
                            {
                                newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDateTimeValue).Json;
                            }
                            break;
                        default:
                            //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                            newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(request.Value).Json;
                            break;
                    }
                }
                parentNode.DataVariables.Add(newDataVariableModel);
                activeNodesetModel.UpdateIndices();
                return Ok(new ApiPropertyModel(newDataVariableModel));
            }
            return BadRequest("A dataVariable with this name exists.");
        }
    }
}
