using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Opc.Ua;
using Opc.Ua.Export.v1.Responses;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using OPC_UA_Nodeset_WebAPI.Model.v1.Requests;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace OPC_UA_Nodeset_WebAPI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/nodeset-model")]
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
        [ProducesResponseType(200, Type = typeof(Dictionary<string, NodeSetModelResponse>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult GetById(string id)
        {
            var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
            {
                return activeNodesetProjectInstanceResult;
            }
            var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;
            var returnObject = new Dictionary<string, NodeSetModelResponse>();
            foreach (var aNodeSetKeyValue in activeNodeSetProjectInstance.NodeSetModels)
            {
                returnObject.Add(aNodeSetKeyValue.Key.Replace("/", ""), new NodeSetModelResponse(aNodeSetKeyValue.Value));
            }
            return Ok(returnObject);
        }

        /// <summary>
        /// Loads a nodeset file from the server.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpPost("load-xml-from-server-async")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, NodeSetModelResponse>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> LoadNodesetXmlFromServerAsync([FromBody] NodesetFile request)
        {
            return await TryLoadNodesetXmlFromServerAsync(request.ProjectId, request.Uri);
        }

        /// <summary>
        /// Loads a nodeset file from a file upload.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        //https://medium.com/@niteshsinghal85/testing-file-upload-with-swagger-in-asp-net-core-90269bc24fe8
        [HttpPost("upload-xml-from-file-async")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, NodeSetModelResponse>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        // https://stackoverflow.com/questions/38698350/increase-upload-file-size-in-asp-net-core
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = Int32.MaxValue, ValueLengthLimit = Int32.MaxValue)]
        public async Task<IActionResult> UploadNodesetXmlFromFileAsync([FromBody] NodesetFile request)
        {
            if (request.File == null)
            {
                return BadRequest("Invalid request: File is required.");
            }
            var id = request.ProjectId;
            var file = request.File;
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }
            var response = await TryLoadNodesetXmlFromServerAsync(id, filePath);
            System.IO.File.Delete(filePath);
            return response;
        }

        /// <summary>
        /// Loads a nodeset file from a string that is encoded using base64.
        /// </summary>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        //https://medium.com/@niteshsinghal85/testing-file-upload-with-swagger-in-asp-net-core-90269bc24fe8
        [HttpPost("upload-xml-from-base-64")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, NodeSetModelResponse>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        // https://stackoverflow.com/questions/38698350/increase-upload-file-size-in-asp-net-core
        [DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = Int32.MaxValue, ValueLengthLimit = Int32.MaxValue)]
        public async Task<IActionResult> UploadNodesetXmlFromBase64([FromBody] NodesetFile request)
        {
            if (request.XmlBase64 == null)
            {
                return BadRequest("Invalid request: parameter XmlBase64 is required.");
            }
            var id = request.ProjectId;
            var xmlBase64 = request.XmlBase64;
            var filePath = Path.GetTempFileName();
            var valueBytes = Convert.FromBase64String(xmlBase64);
            var xml = Encoding.UTF8.GetString(valueBytes);
            System.IO.File.WriteAllText(filePath, xml);

            try
            {
                var response = await TryLoadNodesetXmlFromServerAsync(id, filePath);
                System.IO.File.Delete(filePath);
                return response;
            }
            catch (Exception ex)
            {
                System.IO.File.Delete(filePath);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Creates a blank nodeset model.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Returns a newly created blank nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, NodeSetModelResponse>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> HttpPost([FromBody] NodesetFile request)
        {
            if (request.apiNodeSetInfo == null)
            {
                return BadRequest("Invalid request: apiNodeSetInfo is required.");
            }
            var id = request.ProjectId;
            var apiNodeSetInfo = request.apiNodeSetInfo;
            var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
            if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
            {
                return activeNodesetProjectInstanceResult;
            }
            var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;

            var modelUriResultString = activeNodeSetProjectInstance.AddNewNodeSet(apiNodeSetInfo.ModelUri);
            if (modelUriResultString.StartsWith("Error"))
            {
                activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Fail: Add new NodeSetModel '{apiNodeSetInfo.ModelUri}'. {modelUriResultString}");
                return BadRequest($"{apiNodeSetInfo.ModelUri} - {modelUriResultString}");
            }
            activeNodeSetProjectInstance.NodeSetModels[modelUriResultString].PublicationDate = apiNodeSetInfo.PublicationDate;
            activeNodeSetProjectInstance.NodeSetModels[modelUriResultString].Version = apiNodeSetInfo.Version;

            var aNodesetModel = new NodeSetModelResponse(activeNodeSetProjectInstance.NodeSetModels[modelUriResultString]);
            activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Success: Add new NodeSetModel '{apiNodeSetInfo.ModelUri}'.");
            return Ok(new Dictionary<string, NodeSetModelResponse> { { modelUriResultString.Replace("/", ""), aNodesetModel } });
        }

        /// <summary>
        /// Returns a nodeset xml file 
        /// </summary>
        /// <returns>Returns a nodeset xml file.</returns>
        /// <response code="200">The nodeset was successfully delivered.</response>
        [HttpPost("generate-xml")]
        [Produces("application/xml")]
        [ProducesResponseType(200, Type = typeof(ConcurrentDictionary<string, NodeSetInfoWithDependenciesResponse>))]
        public IActionResult GenerateXml([FromBody] NodesetFile request)
        {
            if (request.Uri == null)
            {
                return BadRequest("Invalid request: Uri is required.");
            }

            var id = request.ProjectId;
            var uri = request.Uri;

            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }

            var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
            var activeNodesetProject = (((ApplicationInstance.GetNodeSetProjectInstance(id)) as ObjectResult).Value as NodeSetProjectInstance).NodeSetModels;

            activeNodesetModel.UpdateIndices();

            var exportedNodeSetXml = UANodeSetModelExporter.ExportNodeSetAsXml(activeNodesetModel, activeNodesetProject);
            var filePath = Path.GetTempFileName();
            System.IO.File.WriteAllText(filePath, exportedNodeSetXml);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            System.IO.File.Delete(filePath);
            return Ok(xmlDoc);
        }

        /// <summary>
        /// Helper method to load a nodeset from the server and handle its result.
        /// </summary>
        /// <param name="id">The ID of the nodeset project.</param>
        /// <param name="uri">The URI of the nodeset file.</param>
        /// <returns>An IActionResult representing the outcome.</returns>
        private async Task<IActionResult> TryLoadNodesetXmlFromServerAsync(string id, string uri)
        {
            try
            {
                var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;

                if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
                {
                    return activeNodesetProjectInstanceResult;
                }

                var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;
                var modelUriResultString = await activeNodeSetProjectInstance.LoadNodeSetFromFileOnServerAsync(uri);
                var aNodesetModel = new NodeSetModelResponse(activeNodeSetProjectInstance.NodeSetModels[modelUriResultString]);
                activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Success: Add NodeSetModel from file '{uri}'.");

                return Ok(new Dictionary<string, NodeSetModelResponse> { { modelUriResultString.Replace("/", ""), aNodesetModel } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load nodeset from URI: {uri}");
                return BadRequest($"Error loading nodeset from URI '{uri}': {ex.Message}");
            }
        }

        /// <summary>
        /// Removes an existing nodeset model from a nodeset project.
        /// </summary>
        /// <returns>A confirmation that the existing nodeset model was remove from the project</returns>
        [HttpDelete]
        [ProducesResponseType(200, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult Destroy([FromBody] NodesetFile request)
        {
            if (string.IsNullOrEmpty(request.Uri))
            {
                return BadRequest("Invalid request: uri is required.");
            }
            if (string.IsNullOrEmpty(request.ProjectId))
            {
                return BadRequest("Invalid request: id is required.");
            }
            var id = request.ProjectId;
            var uri = request.Uri;
            var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;
            if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
            {
                return activeNodesetProjectInstanceResult;
            }
            var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;

            var modelUriResultString = activeNodeSetProjectInstance.RemoveNodeSet(uri);
            if (modelUriResultString.StartsWith("Error"))
            {
                activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Fail: Remove NodeSetModel '{uri}'. {modelUriResultString}");
                return BadRequest($"{uri} - {modelUriResultString}");
            }
            activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Success: Remove NodeSetModel '{uri}'.");
            return Ok(new Dictionary<string, NodeSetModelResponse> { { modelUriResultString.Replace("/", ""), new NodeSetModelResponse() } });
        }
    }
}
