using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
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

        private NodeSetProjectInstance ActiveNodeSetProjectInstance(string id)
        {
            NodeSetProjectInstance aNodesetProjectInstance;
            if (ApplicationInstance.NodeSetProjectInstances.TryGetValue(id, out aNodesetProjectInstance))
            {
                return aNodesetProjectInstance;
            }
            else
            {
                return null; // because the project doesn't exist
            }
        }

        public NodesetModelController(ILogger<NodesetModelController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        [HttpGet]
        public Dictionary<string, ApiNodeSetModel> Get(string id)
        {
            var activeNodeSetProjectInstance = ActiveNodeSetProjectInstance(id);
            if (activeNodeSetProjectInstance == null)
            {
                return null;
            }

            var returnObject = new Dictionary<string, ApiNodeSetModel>();
            foreach(var aNodeSetKeyValue in activeNodeSetProjectInstance.NodeSetModels)
            {
                returnObject.Add(aNodeSetKeyValue.Key.Replace("/", ""), new ApiNodeSetModel(aNodeSetKeyValue.Value));
            }
            return returnObject;
        }

        [HttpPost("LoadNodesetXmlFromServerAsync")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> LoadNodesetXmlFromServerAsync(string id, string uri)
        {
            var activeNodeSetProjectInstance = ActiveNodeSetProjectInstance(id);
            if (activeNodeSetProjectInstance == null)
            {
                var message = $"{id} - not a valid project id.";
                activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Fail: Add NodeSetModel from file '{uri}'. {message}");
                return NotFound(message);
            }

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


        [HttpPut]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> PutAsync(string id, string domain, string name)
        {
            var aModelUri = $"{domain}{(domain.EndsWith("/") ? "" : "/")}{name}{(name.EndsWith("/") ? "" : "/")}";
            var activeNodeSetProjectInstance = ActiveNodeSetProjectInstance(id);
            if (activeNodeSetProjectInstance == null)
            {
                var message = $"{id} - not a valid project id.";
                activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Fail: Add new NodeSetModel '{aModelUri}'. {message}");
                return NotFound(message);
            }

            var modelUriResultString = activeNodeSetProjectInstance.AddNewNodeSet(domain, name);
            if(modelUriResultString.StartsWith("Error"))
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
