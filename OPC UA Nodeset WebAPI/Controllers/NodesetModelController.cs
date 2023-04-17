using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Controllers
{
    [ApiController]
    [Route("NodesetProject/{id}/[controller]")]
    public class NodesetModelController : ControllerBase
    {

        private readonly ILogger<NodesetModelController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public NodesetModelController(ILogger<NodesetModelController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        /// <summary>
        /// Retrieves all loaded nodeset models for a nodeset project.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns all loaded nodeset models for a nodeset project.</returns>
        /// <response code="200">All nodeset models were successfully retrieved for a nodeset project.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id)
        {
            var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
            {
                return activeNodesetProjectInstanceResult;
            }
            else
            {
                var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;
                var returnObject = new Dictionary<string, ApiNodeSetModel>();
                foreach (var aNodeSetKeyValue in activeNodeSetProjectInstance.NodeSetModels)
                {
                    returnObject.Add(aNodeSetKeyValue.Key.Replace("/", ""), new ApiNodeSetModel(aNodeSetKeyValue.Value));
                }
                return Ok(returnObject);

            }


        }

        /// <summary>
        /// Loads a nodeset file from the server.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri"></param>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpPost("LoadNodesetXmlFromServerAsync")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> LoadNodesetXmlFromServerAsync(string id, string uri)
        {
            var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
            {
                return activeNodesetProjectInstanceResult;
            }
            else
            {
                var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;
                var modelUriResultString = await activeNodeSetProjectInstance.LoadNodeSetFromFileOnServerAsync(uri);
                if(modelUriResultString.StartsWith("Error"))
                {
                    activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Fail: Add NodeSetModel from file '{uri}'. {modelUriResultString}");
                    return BadRequest($"{uri} - {modelUriResultString}");
                }
                else
                {
                    var aNodesetModel = new ApiNodeSetModel(activeNodeSetProjectInstance.NodeSetModels[modelUriResultString]);
                    activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Success: Add NodeSetModel from file '{uri}'.");
                    return Ok(new Dictionary<string, ApiNodeSetModel> { { modelUriResultString.Replace("/", ""), aNodesetModel } });
                }
            }


        }

        /// <summary>
        /// Loads a nodeset file from a file upload.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        //https://medium.com/@niteshsinghal85/testing-file-upload-with-swagger-in-asp-net-core-90269bc24fe8
        [HttpPost("UploadNodesetXmlFromFileAsync")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]

        public async Task<IActionResult> UploadNodesetXmlFromFileAsync(string id, IFormFile file)
        {
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            var response = await LoadNodesetXmlFromServerAsync(id, filePath);
            System.IO.File.Delete(filePath);

            return response;
        }


        /// <summary>
        /// Creates a blank nodeset model.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="domain"></param>
        /// <param name="name"></param>
        /// <returns>Returns a newly created blank nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpPut]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> PutAsync(string id, string domain, string name)
        {
            var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
            {
                return activeNodesetProjectInstanceResult;
            }
            else
            {
                var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;

                var aModelUri = $"{domain}{(domain.EndsWith("/") ? "" : "/")}{name}{(name.EndsWith("/") ? "" : "/")}";

                var modelUriResultString = activeNodeSetProjectInstance.AddNewNodeSet(domain, name);
                if (modelUriResultString.StartsWith("Error"))
                {
                    activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Fail: Add new NodeSetModel '{aModelUri}'. {modelUriResultString}");
                    return BadRequest($"{aModelUri} - {modelUriResultString}");
                }
                else
                {
                    var aNodesetModel = new ApiNodeSetModel(activeNodeSetProjectInstance.NodeSetModels[modelUriResultString]);
                    activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Success: Add new NodeSetModel '{aModelUri}'.");
                    return Ok(new Dictionary<string, ApiNodeSetModel> { { modelUriResultString.Replace("/", ""), aNodesetModel } });
                }
            }
        }

    }
}
