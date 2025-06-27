using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using OPC_UA_Nodeset_WebAPI.Model.v1.Requests;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/property")]
    public class PropertyController : AbstractBaseController
    {
        private readonly ILogger<ProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public PropertyController(ILogger<ProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet("{id}/{uri}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, PropertyResponse>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult Get(string id, string uri)
        {
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }
            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
            var returnObject = new List<PropertyResponse>();
            foreach (var aProperty in activeNodesetModel.GetProperties())
            {
                returnObject.Add(new PropertyResponse(aProperty));
            }
            return Ok(returnObject);
        }

        [HttpGet("GetByNodeId")]
        [ProducesResponseType(200, Type = typeof(PropertyResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {
            return ApplicationInstance.GetNodeApiModelByNodeId(id, uri, nodeId, "PropertyModel");
        }

        [HttpPatch]
        [ProducesResponseType(200, Type = typeof(PropertyResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PatchByNodeId([FromBody] PropertyRequest request)
        {
            var id = request.ProjectId;
            var uri = request.Uri;
            var nodeId = request.ParentNodeId;
            var propertiesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
            {
                return propertiesListResult;
            }

            var propertiesList = propertiesListResult.Value as List<PropertyResponse>;
            var existingProperty = propertiesList.FirstOrDefault(x => x.NodeId == nodeId);

            if (existingProperty == null)
            {
                return NotFound("The node id does not exist.");
            }

            // add new property
            var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
            var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

            // patch existing property
            existingProperty.PropertyModel.DisplayName = new List<NodeModel.LocalizedText> { request.DisplayName };
            existingProperty.PropertyModel.BrowseName = request.BrowseName;
            existingProperty.PropertyModel.Description = new List<NodeModel.LocalizedText> { request.Description };

            // look up data type
            //var aDataType = activeProjectInstance.UaBaseModel.AllNodesByNodeId[request.DataTypeNodeId];
            //var nodeFromDataTypeNodeId = new UaNodeResponse { NodeId = request.DataTypeNodeId };
            //var aDataType = activeProjectInstance.NodeSetModels.FirstOrDefault(x=>x.Value.ModelUri== nodeFromDataTypeNodeId.NameSpace).Value.AllNodesByNodeId[request.DataTypeNodeId];
            var aDataType = activeProjectInstance.GetNodeModelByNodeId(request.DataTypeNodeId);

            // patch datatype and value
            if (request.Value != null)
            {
                switch (aDataType.DisplayName.First().Text)
                {
                    case "String":
                        existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "String");
                        existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(request.Value.ToString()).Json;
                        break;
                    case "Integer":
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "SByte":
                        existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                        int aIntValue;
                        if (Int32.TryParse(request.Value, out aIntValue))
                        {
                            existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aIntValue).Json;
                        }
                        break;
                    case "Float":
                    case "Double":
                        existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Double");
                        double aDoubleValue;
                        if (double.TryParse(request.Value, out aDoubleValue))
                        {
                            existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDoubleValue).Json;
                        }
                        break;
                    case "Boolean":
                    case "Bool":
                        existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Boolean");
                        bool aBoolValue;
                        if (bool.TryParse(request.Value.Trim(), out aBoolValue))
                        {
                            existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aBoolValue).Json;
                        }
                        break;
                    case "DateTime":
                    case "UtcTime":
                        existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "DateTime");
                        DateTime aDateTimeValue;
                        if (DateTime.TryParse(request.Value, out aDateTimeValue))
                        {
                            existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDateTimeValue).Json;
                        }
                        break;
                    default:
                        if (existingProperty.PropertyModel.DataType.SuperType.NodeId == "nsu=http://opcfoundation.org/UA/;i=29")
                        {
                            existingProperty.PropertyModel.DataType = aDataType as DataTypeModel;
                            existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(Int32.Parse(request.Value)).Json;
                        }
                        break;
                }
            }

            activeNodesetModel.UpdateIndices();
            return Ok(new PropertyResponse(existingProperty.PropertyModel));
        }


        [HttpGet("ByParentNodeId")]
        [ProducesResponseType(200, Type = typeof(List<PropertyResponse>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByParentNodeId(string id, string uri, string parentNodeId)
        {
            var aParentNodeResult = ApplicationInstance.GetNodeModelByNodeId(id, uri, parentNodeId) as ObjectResult;

            if (StatusCodes.Status200OK != aParentNodeResult.StatusCode)
            {
                return aParentNodeResult;
            }
            else
            {
                NodeModel aNodeModel = aParentNodeResult.Value as NodeModel;

                List<VariableModel> properties = new List<VariableModel>();
                switch (aNodeModel)
                {
                    case ObjectTypeModel aObjectTypeModel:
                        properties = aObjectTypeModel.Properties;
                        break;
                    case ObjectModel aObjectModel:
                        properties = aObjectModel.Properties;
                        break;
                    case DataVariableModel aDataVariableModel:
                        properties = aDataVariableModel.Properties;
                        break;
                    default:
                        break;
                }

                var returnObject = properties.Select(x => new PropertyResponse(x));

                return Ok(returnObject);

            }
        }


        [HttpPost]
        [ProducesResponseType(200, Type = typeof(PropertyResponse))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> HttpPost([FromBody] PropertyRequest request)
        {
            try
            {
                var id = request.ProjectId;
                var uri = request.Uri;
                var propertiesListResult = Get(id, uri) as ObjectResult;

                if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
                {
                    return propertiesListResult;
                }
                var propertiesList = propertiesListResult.Value as List<PropertyResponse>;
                FindOpcType<PropertyResponse>(propertiesList, request);

                // add new property
                var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                // look up parent object
                var parentNode = activeProjectInstance.GetNodeModelByNodeId(request.ParentNodeId);

                // look up data type
                var aDataType = activeProjectInstance.GetNodeModelByNodeId(request.DataTypeNodeId) as DataTypeModel;

                var newPropertyModel = new PropertyModel
                {
                    NodeSet = activeNodesetModel,
                    NodeId = UaNodeResponse.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                    Parent = parentNode,
                    DisplayName = new List<NodeModel.LocalizedText> { request.DisplayName },
                    BrowseName = request.BrowseName,
                    Description = new List<NodeModel.LocalizedText> { request.Description == null ? "" : request.Description },
                    DataType = aDataType as DataTypeModel
                };

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
                            int aIntValue;
                            if (Int32.TryParse(request.Value, out aIntValue))
                            {
                                newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aIntValue).Json;
                            }
                            break;
                        case "Float":
                        case "Double":
                            double aDoubleValue;
                            if (double.TryParse(request.Value, out aDoubleValue))
                            {
                                newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDoubleValue).Json;
                            }
                            break;
                        case "Duration":
                            double durationSeconds;
                            if (double.TryParse(request.Value, out durationSeconds))
                            {
                                TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                                newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(durationSeconds).Json;
                            }
                            break;
                        case "Boolean":
                        case "Bool":
                            Boolean aBoolValue;
                            if (Boolean.TryParse(request.Value, out aBoolValue))
                            {
                                newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aBoolValue).Json;
                            }
                            break;
                        case "DateTime":
                        case "UtcTime":
                            DateTime aDateTimeValue;
                            if (DateTime.TryParse(request.Value, out aDateTimeValue))
                            {
                                newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDateTimeValue).Json;
                            }
                            break;
                        default:
                            //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                            newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(request.Value).Json;
                            break;
                    }
                }

                parentNode.Properties.Add(newPropertyModel);
                activeNodesetModel.UpdateIndices();
                return Ok(new PropertyResponse(newPropertyModel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating Property");
                return BadRequest("Error creating new property: " + ex.Message);
            }
        }

        [HttpPost("bulk-processing")]
        [ProducesResponseType(200, Type = typeof(IActionResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> BulkProcessing([FromBody] BulkPropertyRequest request)
        {
            try
            {
                string id = request.ProjectId;
                string uri = request.Uri;
                string parentNodeId = request.ParentNodeId;
                var propertiesListResult = Get(id, uri) as ObjectResult;

                if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
                {
                    throw new Exception($"Error retrieving objects for project {id} and URI {uri}");
                }

                foreach (var type in request.Types)
                {
                    var propertiesList = propertiesListResult.Value as List<PropertyResponse>;
                    FindOpcType<PropertyResponse>(propertiesList, request, type);

                    // add new property
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    // look up parent object
                    var parentNode = activeProjectInstance.GetNodeModelByNodeId(parentNodeId);

                    // look up data type
                    var aDataType = activeProjectInstance.GetNodeModelByNodeId(type.DataTypeNodeId) as DataTypeModel;

                    var newPropertyModel = new PropertyModel
                    {
                        NodeSet = activeNodesetModel,
                        NodeId = UaNodeResponse.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                        Parent = parentNode,
                        DisplayName = new List<NodeModel.LocalizedText> { type.DisplayName },
                        BrowseName = type.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { type.Description == null ? "" : type.Description },
                        DataType = aDataType as DataTypeModel
                    };

                    // add value
                    if (type.Value != null)
                    {
                        switch (aDataType.DisplayName.First().Text)
                        {
                            case "Integer":
                            case "Int16":
                            case "Int32":
                            case "Int64":
                            case "SByte":
                                int aIntValue;
                                if (Int32.TryParse(type.Value, out aIntValue))
                                {
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aIntValue).Json;
                                }
                                break;
                            case "Float":
                            case "Double":
                                double aDoubleValue;
                                if (double.TryParse(type.Value, out aDoubleValue))
                                {
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDoubleValue).Json;
                                }
                                break;
                            case "Duration":
                                double durationSeconds;
                                if (double.TryParse(type.Value, out durationSeconds))
                                {
                                    TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(durationSeconds).Json;
                                }
                                break;
                            case "Boolean":
                            case "Bool":
                                Boolean aBoolValue;
                                if (Boolean.TryParse(type.Value, out aBoolValue))
                                {
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aBoolValue).Json;
                                }
                                break;
                            case "DateTime":
                            case "UtcTime":
                                DateTime aDateTimeValue;
                                if (DateTime.TryParse(type.Value, out aDateTimeValue))
                                {
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDateTimeValue).Json;
                                }
                                break;
                            default:
                                //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                                newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(type.Value).Json;
                                break;
                        }
                    }

                    parentNode.Properties.Add(newPropertyModel);
                    activeNodesetModel.UpdateIndices();
                }
                return Ok(new { Message = "good" });
            }
            catch (Exception exception)
            {
                return BadRequest("Error creating new property: " + exception.Message);
            }
        }
    }
}
