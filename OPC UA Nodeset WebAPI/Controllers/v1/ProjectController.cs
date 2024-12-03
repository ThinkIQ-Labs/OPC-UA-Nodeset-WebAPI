using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Collections.Concurrent;
using System.Linq;
using Opc.Ua.Export;

namespace OPC_UA_Nodeset_WebAPI.api.v1.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProjectController : ControllerBase
    {

        private readonly ILogger<ProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public ProjectController(ILogger<ProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        /// <summary>
        /// Returns all current nodeset projects.
        /// </summary>
        /// <returns>Returns a dictionary of current projects.</returns>
        /// <response code="200">All nodeset projects were successfully retrieved.</response>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetProject>))]
        public IActionResult Index()
        {
            var returnObject = new Dictionary<string, ApiNodeSetProject>();
            foreach (var aKeyValue in ApplicationInstance.NodeSetProjectInstances)
            {
                returnObject.Add(aKeyValue.Key, new ApiNodeSetProject(aKeyValue.Value));
            }
            return Ok(returnObject);
        }

        /// <summary>
        /// Creates a new nodeset project.
        /// </summary>
        /// <param name="request">The NodesetTrequest to create project for.</param>
        /// <returns>Returns the created project.</returns>
        /// <response code="200">Project was successfully created.</response>
        /// <response code="400">The project was not created.</response>
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(List<ApiNodeSetProject>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        public IActionResult Store([FromBody] NodesetRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.name) || string.IsNullOrWhiteSpace(request.owner))
            {
                return BadRequest("Invalid request: Name and Owner are required.");
            }

            var name = request.name;
            var owner = request.owner;
            var key = Guid.NewGuid().ToString().Split("-")[0];
            var newProject = new NodeSetProjectInstance(name, owner, key);
            var result = ApplicationInstance.NodeSetProjectInstances.TryAdd(key, newProject);

            if (result)
            {
                var getByIdResult = GetById(key) as OkObjectResult;
                var aNodesetProject = getByIdResult.Value as Dictionary<string, ApiNodeSetProject>;
                aNodesetProject.First().Value.AddToLog($"Project '{name}'({aNodesetProject.First().Key}) created successfully.");

                return Ok(aNodesetProject.First().Value);
            }

            return BadRequest($"{name} - this did not work.");
        }

        /// <summary>
        /// Returns a nodeset project by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="200">The nodeset project was successfully retrieved.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetProject>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id)
        {
            NodeSetProjectInstance aNodesetProjectInstance;
            var projectExists = ApplicationInstance.NodeSetProjectInstances.TryGetValue(id, out aNodesetProjectInstance);
            if (projectExists)
            {
                return Ok(new Dictionary<string, ApiNodeSetProject> { { id, new ApiNodeSetProject(aNodesetProjectInstance) } });
            }
            return NotFound($"{id} - not a valid project id.");
        }

        /// <summary>
        /// Deletes a nodeset project by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns the deleted project.</returns>
        /// <response code="200">Project was successfully deleted.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult Destroy(string id)
        {
            try
            {
                // check if the key is good first
                var getByIdResult = GetById(id) as ObjectResult;

                if (StatusCodes.Status200OK != getByIdResult.StatusCode)
                {
                    return NotFound($"{id} - not a valid project id.");
                }
                // id is good attempt to delete project
                NodeSetProjectInstance aNodesetProjectInstance;
                var result = false;
                var counter = 0;
                var maxAttempts = 10;
                while (result == false && counter < maxAttempts)
                {
                    result = ApplicationInstance.NodeSetProjectInstances.TryRemove(id, out aNodesetProjectInstance);
                    if (result)
                    {
                        var aNodesetProject = new Dictionary<string, ApiNodeSetProject> { { id, new ApiNodeSetProject(aNodesetProjectInstance) } };
                        aNodesetProject.First().Value.AddToLog($"Project '{aNodesetProject.First().Value.Name}'({id}) deleted successfully.");
                        return Ok($"{id} - project was successfully deleted.");
                    }
                    counter++;
                    Thread.Sleep(100);
                }
                return BadRequest($"{id} - removal was unsuccessful {maxAttempts}x.");
            }
            catch (Exception ex)
            {
                return BadRequest($"{id} - {ex.Message}");
            }
        }
    }
}
