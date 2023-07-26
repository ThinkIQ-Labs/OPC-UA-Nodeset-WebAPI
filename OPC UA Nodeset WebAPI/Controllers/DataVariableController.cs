using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/NodesetModel/{uri}/[controller]")]
    public class DataVariableController : ControllerBase
    {
        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public DataVariableController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiDataVariableModel>))]
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
                var returnObject = new List<ApiDataVariableModel>();
                foreach (var aDataVariable in activeNodesetModel.GetDataVariables())
                {
                    returnObject.Add(new ApiDataVariableModel(aDataVariable));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiDataVariableModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id, string uri, string nodeId)
        {
            var dataVariablesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataVariablesListResult.StatusCode)
            {
                return dataVariablesListResult;
            }
            else
            {
                var dataVariablesList = dataVariablesListResult.Value as List<ApiDataVariableModel>;
                var returnObject = dataVariablesList.FirstOrDefault(x => x.NodeId == HttpUtility.UrlDecode(nodeId));
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

        [HttpGet("ByParentNodeId")]
        [ProducesResponseType(200, Type = typeof(List<ApiDataVariableModel>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByParentNodeId(string id, string uri, string parentNodeId)
        {
            var dataVariablesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataVariablesListResult.StatusCode)
            {
                return dataVariablesListResult;
            }
            else
            {
                var dataVariablesList = dataVariablesListResult.Value as List<ApiDataVariableModel>;
                var returnObject = dataVariablesList.Where(x => x.ParentNodeId == parentNodeId);
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
        [ProducesResponseType(200, Type = typeof(ApiDataVariableModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PutAsync(string id, string uri, [FromBody] ApiNewDataVariableModel apiDataVariableModel)
        {

            var dataVariablesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataVariablesListResult.StatusCode)
            {
                return dataVariablesListResult;
            }
            else
            {
                var dataVariablesList = dataVariablesListResult.Value as List<ApiDataVariableModel>;
                var existingDataVariable = dataVariablesList.Where(x => x.ParentNodeId == apiDataVariableModel.ParentNodeId).FirstOrDefault(x => x.DisplayName == apiDataVariableModel.DisplayName);
                if (existingDataVariable == null)
                {
                    // add new dataVariable
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    // look up parent object
                    var parentNode = activeProjectInstance.GetNodeModelByNodeId(apiDataVariableModel.ParentNodeId);

                    // look up data type
                    var aDataType = activeProjectInstance.GetNodeModelByNodeId(apiDataVariableModel.DataTypeNodeId) as DataTypeModel;

                    var newDataVariableModel = new DataVariableModel
                    {
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                        Parent = parentNode,
                        DisplayName = new List<NodeModel.LocalizedText> { apiDataVariableModel.DisplayName },
                        BrowseName = apiDataVariableModel.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { apiDataVariableModel.Description == null ? "" : apiDataVariableModel.Description },
                        DataType = aDataType as DataTypeModel
                    };

                    // add value
                    if (apiDataVariableModel.Value != null)
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
                                if (Int32.TryParse(apiDataVariableModel.Value, out aIntValue))
                                {
                                    newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aIntValue);
                                }
                                break;
                            case "Boolean":
                            case "Bool":
                                //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Boolean");
                                Boolean aBoolValue;
                                if (Boolean.TryParse(apiDataVariableModel.Value, out aBoolValue))
                                {
                                    newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aBoolValue);
                                }
                                break;
                            case "DateTime":
                            case "UtcTime":
                                //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "DateTime");
                                DateTime aDateTimeValue;
                                if (DateTime.TryParse(apiDataVariableModel.Value, out aDateTimeValue))
                                {
                                    newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(aDateTimeValue);
                                }
                                break;
                            default:
                                //newPropertyModel.DataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == "Int32");
                                newDataVariableModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(apiDataVariableModel.Value);
                                break;
                        }
                    }



                    parentNode.DataVariables.Add(newDataVariableModel);
                    activeNodesetModel.UpdateIndices();
                    return Ok(new ApiPropertyModel(newDataVariableModel));
                }
                else
                {
                    return BadRequest("A dataVariable with this name exists.");
                }
            }
        }


    }
}
