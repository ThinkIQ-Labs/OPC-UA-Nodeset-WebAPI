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
    public class DataTypeController : ControllerBase
    {
        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public DataTypeController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiDataTypeModel>))]
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
                var returnObject = new List<ApiDataTypeModel>();
                foreach (var aDataType in activeNodesetModel.DataTypes)
                {
                    returnObject.Add(new ApiDataTypeModel(aDataType));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiDataTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByNodeId(string id, string uri, string nodeId)
        {
            var dataTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataTypesListResult.StatusCode)
            {
                return dataTypesListResult;
            }
            else
            {
                var dataTypes = dataTypesListResult.Value as List<ApiDataTypeModel>;
                var returnObject = dataTypes.FirstOrDefault(x => x.NodeId == HttpUtility.UrlDecode(nodeId));
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
        [ProducesResponseType(200, Type = typeof(List<ApiDataTypeModel>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByDisplayName(string id, string uri, string displayName)
        {
            var dataTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataTypesListResult.StatusCode)
            {
                return dataTypesListResult;
            }
            else
            {
                var dataTypes = dataTypesListResult.Value as List<ApiDataTypeModel>;
                var returnObject = dataTypes.Where(x => x.DisplayName == displayName).ToList();
                return Ok(returnObject);
            }
        }

        [HttpPut]
        [ProducesResponseType(200, Type = typeof(ApiDataTypeModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult PutAsync(string id, string uri, [FromBody] ApiNewDataTypeModel apiDataTypeModel)
        {

            var dataTypesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != dataTypesListResult.StatusCode)
            {
                return dataTypesListResult;
            }
            else
            {
                var dataTypes = dataTypesListResult.Value as List<ApiDataTypeModel>;
                var existingDataType = dataTypes.FirstOrDefault(x => x.DisplayName == apiDataTypeModel.DisplayName);
                if (existingDataType == null)
                {
                    // add new data type
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    var newDataTypeModel = new DataTypeModel
                    {
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace((activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++).ToString(), activeNodesetModel.ModelUri),
                        SuperType = activeProjectInstance.GetNodeModelByNodeId(apiDataTypeModel.SuperTypeNodeId) as DataTypeModel,
                        DisplayName = new List<NodeModel.LocalizedText> { apiDataTypeModel.DisplayName == null ? "" : apiDataTypeModel.DisplayName },
                        BrowseName = apiDataTypeModel.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { apiDataTypeModel.Description == null ? "" : apiDataTypeModel.Description },
                        EnumFields = apiDataTypeModel.EnumFields.Select(x => new DataTypeModel.UaEnumField
                        {
                            Name = x.Name,
                            Value = x.Value,
                            Description = x.Description == null ? new List<NodeModel.LocalizedText>() : new List<NodeModel.LocalizedText> { x.Description },
                            DisplayName = x.DisplayName == null ? new List<NodeModel.LocalizedText>() :new List<NodeModel.LocalizedText> { x.DisplayName }
                        }).ToList()
                    };

                    activeNodesetModel.DataTypes.Add(newDataTypeModel);
                    activeNodesetModel.UpdateIndices();
                    return Ok(new ApiDataTypeModel(newDataTypeModel));
                }
                else
                {
                    return BadRequest("An data type with this name exists.");
                }
            }
        }


    }
}
