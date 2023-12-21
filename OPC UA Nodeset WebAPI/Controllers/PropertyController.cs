using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/NodesetModel/{uri}/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

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
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }
            else
            {
                var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
                var returnObject = new List<ApiPropertyModel>();
                foreach (var aProperty in activeNodesetModel.GetProperties())
                {
                    returnObject.Add(new ApiPropertyModel(aProperty));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("GetByNodeId")]
        [ProducesResponseType(200, Type = typeof(ApiPropertyModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {

            return ApplicationInstance.GetNodeApiModelByNodeId(id, uri, nodeId, "PropertyModel");

            //var propertiesListResult = Get(id, uri) as ObjectResult;

            //if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
            //{
            //    return propertiesListResult;
            //}
            //else
            //{
            //    var propertiesList = propertiesListResult.Value as List<ApiPropertyModel>;
            //    var returnObject = propertiesList.FirstOrDefault(x=>x.NodeId== nodeId);
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

        [HttpPatch("PatchByNodeId")]
        [ProducesResponseType(200, Type = typeof(ApiPropertyModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PatchByNodeId(string id, string uri, string nodeId, [FromBody] ApiNewPropertyModel apiPropertyModel)
        {
            var propertiesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
            {
                return propertiesListResult;
            }
            else
            {

                var propertiesList = propertiesListResult.Value as List<ApiPropertyModel>;
                var existingProperty = propertiesList.FirstOrDefault(x=>x.NodeId== nodeId);
                if (existingProperty != null)
                {
                    // add new property
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    // patch existing property
                    existingProperty.PropertyModel.DisplayName = new List<NodeModel.LocalizedText> { apiPropertyModel.DisplayName };
                    existingProperty.PropertyModel.BrowseName = apiPropertyModel.BrowseName;
                    existingProperty.PropertyModel.Description = new List<NodeModel.LocalizedText> { apiPropertyModel.Description };

                    // look up data type
                    //var aDataType = activeProjectInstance.UaBaseModel.AllNodesByNodeId[apiPropertyModel.DataTypeNodeId];
                    //var nodeFromDataTypeNodeId = new ApiUaNodeModel { NodeId = apiPropertyModel.DataTypeNodeId };
                    //var aDataType = activeProjectInstance.NodeSetModels.FirstOrDefault(x=>x.Value.ModelUri== nodeFromDataTypeNodeId.NameSpace).Value.AllNodesByNodeId[apiPropertyModel.DataTypeNodeId];
                    var aDataType = activeProjectInstance.GetNodeModelByNodeId(apiPropertyModel.DataTypeNodeId);

                    // patch datatype and value
                    if (apiPropertyModel.Value != null)
                    {
                        switch (aDataType.DisplayName.First().Text)
                        {
                            case "String":
                                existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "String");
                                existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(apiPropertyModel.Value.ToString()).Json;
                                break;
                            case "Integer":
                            case "Int16":
                            case "Int32":
                            case "Int64":
                            case "SByte":
                                existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                                int aIntValue;
                                if (Int32.TryParse(apiPropertyModel.Value, out aIntValue))
                                {
                                    existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aIntValue).Json;
                                }
                                break;
                            case "Boolean":
                            case "Bool":
                                existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Boolean");
                                Boolean aBoolValue;
                                if (Boolean.TryParse(apiPropertyModel.Value, out aBoolValue))
                                {
                                    existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aBoolValue).Json;
                                }
                                break;
                            case "DateTime":
                            case "UtcTime":
                                existingProperty.PropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "DateTime");
                                DateTime aDateTimeValue;
                                if (DateTime.TryParse(apiPropertyModel.Value, out aDateTimeValue))
                                {
                                    existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDateTimeValue).Json;
                                }
                                break;
                            default:
if (existingProperty.PropertyModel.DataType.SuperType.NodeId == "nsu=http://opcfoundation.org/UA/;i=29")
{
    existingProperty.PropertyModel.DataType = aDataType as DataTypeModel;
    existingProperty.PropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(Int32.Parse(apiPropertyModel.Value)).Json;
}
                                break;
                        }
                    }

                    activeNodesetModel.UpdateIndices();
                    return Ok(new ApiPropertyModel(existingProperty.PropertyModel));
                }
                else
                {
                    return NotFound("The node id does not exist.");
                }
            }
        }


        [HttpGet("ByParentNodeId")]
        [ProducesResponseType(200, Type = typeof(List<ApiPropertyModel>))]
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

                var returnObject = properties.Select(x => new ApiPropertyModel(x));

                return Ok(returnObject);

            }
        }


        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ApiPropertyModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PutAsync(string id, string uri, [FromBody] ApiNewPropertyModel apiPropertyModel)
        {

            var propertiesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
            {
                return propertiesListResult;
            }
            else
            {
                var propertiesList = propertiesListResult.Value as List<ApiPropertyModel>;
                var existingProperty = propertiesList.Where(x => x.ParentNodeId == apiPropertyModel.ParentNodeId).FirstOrDefault(x => x.DisplayName == apiPropertyModel.DisplayName);
                if (existingProperty == null)
                {
                    // add new property
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    // look up parent object
                    var parentNode = activeProjectInstance.GetNodeModelByNodeId(apiPropertyModel.ParentNodeId);

                    // look up data type
                    var aDataType = activeProjectInstance.GetNodeModelByNodeId(apiPropertyModel.DataTypeNodeId) as DataTypeModel;

                    var newPropertyModel = new PropertyModel
                    {
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                        Parent = parentNode,
                        DisplayName = new List<NodeModel.LocalizedText> { apiPropertyModel.DisplayName },
                        BrowseName = apiPropertyModel.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { apiPropertyModel.Description == null ? "" : apiPropertyModel.Description },
                        DataType = aDataType as DataTypeModel
                    };

                    // add value
                    if (apiPropertyModel.Value != null)
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
                                if (Int32.TryParse(apiPropertyModel.Value, out aIntValue))
                                {
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aIntValue).Json;
                                }
                                break;
                            case "Boolean":
                            case "Bool":
                                //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Boolean");
                                Boolean aBoolValue;
                                if (Boolean.TryParse(apiPropertyModel.Value, out aBoolValue))
                                {
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aBoolValue).Json;
                                }
                                break;
                            case "DateTime":
                            case "UtcTime":
                                //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "DateTime");
                                DateTime aDateTimeValue;
                                if (DateTime.TryParse(apiPropertyModel.Value, out aDateTimeValue))
                                {
                                    newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDateTimeValue).Json;
                                }
                                break;
                            default:
                                //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                                newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(apiPropertyModel.Value).Json;
                                break;
                        }
                    }



                    parentNode.Properties.Add(newPropertyModel);
                    activeNodesetModel.UpdateIndices();
                    return Ok(new ApiPropertyModel(newPropertyModel));
                }
                else
                {
                    return BadRequest("A property with this name exists.");
                }
            }
        }


    }
}
