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
                foreach (var aProperty in activeNodesetModel.Properties)
                {
                    returnObject.Add(new ApiPropertyModel(aProperty));
                }
                return Ok(returnObject);
            }
        }

        [HttpGet("{nodeId}")]
        [ProducesResponseType(200, Type = typeof(ApiPropertyModel))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id, string uri, int nodeId)
        {
            var propertiesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
            {
                return propertiesListResult;
            }
            else
            {
                var propertiesList = propertiesListResult.Value as List<ApiPropertyModel>;
                var returnObject = propertiesList.FirstOrDefault(x=>x.Id== nodeId);
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

        [HttpGet("ByParentId")]
        [ProducesResponseType(200, Type = typeof(List<ApiPropertyModel>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetByParentId(string id, string uri, int parentId)
        {
            var propertiesListResult = Get(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != propertiesListResult.StatusCode)
            {
                return propertiesListResult;
            }
            else
            {
                var propertiesList = propertiesListResult.Value as List<ApiPropertyModel>;
                var returnObject = propertiesList.Where(x=>x.ParentId== parentId);
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
                var properties = propertiesListResult.Value as List<ApiPropertyModel>;
                var existingProperty = properties.Where(x => x.ParentId == apiPropertyModel.ParentId).FirstOrDefault(x => x.DisplayName == apiPropertyModel.DisplayName);
                if (existingProperty == null)
                {
                    // add new property
                    var projectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
                    var activeProjectInstance = projectInstanceResult.Value as NodeSetProjectInstance;

                    var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;
                    var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

                    // look up parent object
                    var parentNode = activeNodesetModel.AllNodesByNodeId[ApiUaNodeModel.GetNodeIdFromIdAndNameSpace(apiPropertyModel.ParentId, activeNodesetModel.ModelUri)];

                    // look up data type
                    var aDataType = activeProjectInstance.UaBaseModel.DataTypes.FirstOrDefault(ot => ot.DisplayName.First().Text == apiPropertyModel.DataType);

                    var newPropertyModel = new PropertyModel
                    {
                        NodeSet = activeNodesetModel,
                        NodeId = ApiUaNodeModel.GetNodeIdFromIdAndNameSpace(activeProjectInstance.NextNodeIds[activeNodesetModel.ModelUri]++, activeNodesetModel.ModelUri),
                        Parent = parentNode,
                        DataType = aDataType,
                        DisplayName = new List<NodeModel.LocalizedText> { apiPropertyModel.DisplayName },
                        BrowseName = apiPropertyModel.BrowseName,
                        Description = new List<NodeModel.LocalizedText> { apiPropertyModel.Description == null ? "" : apiPropertyModel.Description },
                    };

                    // add value
                    if (apiPropertyModel.DefaultValue != null)
                    {
                        newPropertyModel.Value = activeProjectInstance.opcContext.JsonEncodeVariant(apiPropertyModel.DefaultValue);
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
