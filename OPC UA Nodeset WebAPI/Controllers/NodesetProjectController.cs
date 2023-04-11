using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Collections.Concurrent;
using System.Linq;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NodesetProjectController : ControllerBase
    {

        private readonly ILogger<NodesetProjectController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public NodesetProjectController(ILogger<NodesetProjectController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance= applicationInstance;
        }

        /// <summary>
        /// Returns a dictionary of current projects.
        /// </summary>
        /// <returns>Returns a dictionary of current projects.</returns>
        /// <response code="200">Returns a dictionary of current projects.</response>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetProject>))]
        public IActionResult Get()
        {
            var returnObject = new Dictionary<string, ApiNodeSetProject>();
            foreach(var aKeyValue in ApplicationInstance.NodeSetProjectInstances)
            {
                returnObject.Add(aKeyValue.Key, new ApiNodeSetProject(aKeyValue.Value));
            }
            return Ok(returnObject);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string,ApiNodeSetProject>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id)
        {
            NodeSetProjectInstance aNodesetProjectInstance;
            var projectExists = ApplicationInstance.NodeSetProjectInstances.TryGetValue(id, out aNodesetProjectInstance);
            if (projectExists)
            {
                return Ok(new Dictionary<string, ApiNodeSetProject> { { id, new ApiNodeSetProject(aNodesetProjectInstance) } });
            }
            else
            {
                return NotFound($"{id} - not a valid project id.");
            }
        }

        [HttpPut]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetProject>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        public IActionResult Put(string name)
        {
            var key = Guid.NewGuid().ToString().Split("-")[0];
            var result = ApplicationInstance.NodeSetProjectInstances.TryAdd(key, new NodeSetProjectInstance(name));
            if (result)
            {
                var getByIdResult = GetById(key) as OkObjectResult;
                var aNodesetProject = getByIdResult.Value as Dictionary<string, ApiNodeSetProject>;
                aNodesetProject.First().Value.AddToLog($"Project '{name}'({aNodesetProject.First().Key}) created successfully.");
                return Ok(aNodesetProject);
            }
            else
            {
                return BadRequest($"{name} - this did not work.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetProject>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult Delete(string id)
        {
            try
            {
                // check if the key is good first
                var getByIdResult = GetById(id) as ObjectResult;

                if (StatusCodes.Status200OK != getByIdResult.StatusCode)
                {
                    return NotFound($"{id} - not a valid project id.");
                }
                else
                {
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
                            return Ok(aNodesetProject);
                        }
                        counter++;
                        Thread.Sleep(100);
                    }
                    return BadRequest($"{id} - removal was unsuccessful {maxAttempts}x.");
                }
            } catch(Exception ex)
            {
                return BadRequest($"{id} - {ex.Message}");
            }
            



        }
    }
}