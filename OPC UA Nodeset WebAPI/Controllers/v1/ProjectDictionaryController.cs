using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using OPC_UA_Nodeset_WebAPI.Model.v1.Requests;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Collections.Concurrent;
using System.Linq;
using Opc.Ua.Export.v1.Responses;

namespace OPC_UA_Nodeset_WebAPI.api.v1.Controllers
{
    [ApiController]
    [Route("api/v1/project-dictionary")]
    public class ProjectDictionaryController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public ProjectDictionaryController(ILogger<ProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        /// <summary>
        /// Helps to create a project dionary from all the objects, properties, and information of each of the namespaces in 
        /// Order to be constructed property to be consume by the client.
        /// </summary>
        /// <returns>Returns a dictionary of current projects.</returns>
        /// <response code="200">All nodeset projects were successfully retrieved.</response>
        [HttpGet("{id}/{uri}")]
        [ProducesResponseType(200, Type = typeof(ApiCombinedResponse))]
        public IActionResult Get(string id, string uri)
        {
            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }

            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;

            var combinedResponse = new ApiCombinedResponse
            {
                ObjectTypes = PopulateList(activeNodesetModel.ObjectTypes, objectType => new ObjectTypeResponse(objectType)),
                DataVariables = PopulateList(activeNodesetModel.GetDataVariables(), dataVariable => new DataVariableResponse(dataVariable)),
                Properties = PopulateList(activeNodesetModel.GetProperties(), property => new PropertyResponse(property)),
                VariableTypes = PopulateList(activeNodesetModel.VariableTypes, variableType => new VariableTypeResponse(variableType)),
                DataTypes = PopulateList(activeNodesetModel.DataTypes, dataType => new DataTypeResponse(dataType)),
                Objects = PopulateList(activeNodesetModel.GetObjects(), objectModel => new ObjectModelResponse(objectModel))
            };

            return Ok(combinedResponse);
        }

        /**
         * Helps to create a list of objects from the source.
         *
         * @param source The source to be used to populate the list.
         * @param selector The selector to be used to populate the list.
         * @returns a dictionary of current projects.
         */
        private List<TDestination> PopulateList<TSource, TDestination>(IEnumerable<TSource> source, Func<TSource, TDestination> selector)
        {
            return source?.Select(selector).ToList() ?? new List<TDestination>();
        }
    }
}
