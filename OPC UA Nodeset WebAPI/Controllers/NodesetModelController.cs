using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Opc.Ua;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.Model;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

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
                    //returnObject.Add(aNodeSetKeyValue.Key, new ApiNodeSetModel(aNodeSetKeyValue.Value));
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
                    //return Ok(new Dictionary<string, ApiNodeSetModel> { { modelUriResultString, aNodesetModel } });
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
        /// Loads a nodeset file from a string that is encoded using base64.
        /// </summary>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        //https://medium.com/@niteshsinghal85/testing-file-upload-with-swagger-in-asp-net-core-90269bc24fe8
        [HttpPost("UploadNodesetXmlFromBase64")]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]

        public async Task<IActionResult> UploadNodesetXmlFromBase64(string id, [FromBody] UANodeSetBase64Upload data)
        {

            var filePath = Path.GetTempFileName();

            var valueBytes = Convert.FromBase64String(data.XmlBase64);
            var xml = Encoding.UTF8.GetString(valueBytes);

            System.IO.File.WriteAllText(filePath, xml);

            var response = await LoadNodesetXmlFromServerAsync(id, filePath);
            System.IO.File.Delete(filePath);

            return response;
        }


        /// <summary>
        /// Creates a blank nodeset model.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="modelUri"></param>
        /// <returns>Returns a newly created blank nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        /// <response code="404">The project id was not valid.</response>
        [HttpPut]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, ApiNodeSetModel>))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public async Task<IActionResult> PutAsync(string id, [FromBody] ApiNodeSetInfo apiNodeSetInfo)
        {
            var activeNodesetProjectInstanceResult = ApplicationInstance.GetNodeSetProjectInstance(id) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetProjectInstanceResult.StatusCode)
            {
                return activeNodesetProjectInstanceResult;
            }
            else
            {
                var activeNodeSetProjectInstance = activeNodesetProjectInstanceResult.Value as NodeSetProjectInstance;

                var modelUriResultString = activeNodeSetProjectInstance.AddNewNodeSet(apiNodeSetInfo.ModelUri);
                if (modelUriResultString.StartsWith("Error"))
                {
                    activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Fail: Add new NodeSetModel '{apiNodeSetInfo.ModelUri}'. {modelUriResultString}");
                    return BadRequest($"{apiNodeSetInfo.ModelUri} - {modelUriResultString}");
                }
                else
                {
                    activeNodeSetProjectInstance.NodeSetModels[modelUriResultString].PublicationDate = apiNodeSetInfo.PublicationDate;
                    activeNodeSetProjectInstance.NodeSetModels[modelUriResultString].Version = apiNodeSetInfo.Version;

                    var aNodesetModel = new ApiNodeSetModel(activeNodeSetProjectInstance.NodeSetModels[modelUriResultString]);
                    activeNodeSetProjectInstance.Log.Add(DateTime.UtcNow.ToString("o"), $"Success: Add new NodeSetModel '{apiNodeSetInfo.ModelUri}'.");
                    //return Ok(new Dictionary<string, ApiNodeSetModel> { { modelUriResultString, aNodesetModel } });
                    return Ok(new Dictionary<string, ApiNodeSetModel> { { modelUriResultString.Replace("/", ""), aNodesetModel } });
                }
            }
        }

        /// <summary>
        /// Returns a nodeset xml file 
        /// </summary>
        /// <returns>Returns a nodeset xml file.</returns>
        /// <response code="200">The nodeset was successfully delivered.</response>
        [HttpGet("GenerateXml/{uri}")]
        [Produces("application/xml")]
        [ProducesResponseType(200, Type = typeof(ConcurrentDictionary<string, ApiNodeSetInfoWithDependencies>))]
        public IActionResult GenerateXml(string id, string uri)
        {

            var activeNodesetModelResult = ApplicationInstance.GetNodeSetModel(id, uri) as ObjectResult;

            if (StatusCodes.Status200OK != activeNodesetModelResult.StatusCode)
            {
                return activeNodesetModelResult;
            }
            else
            {

                var activeNodesetModel = activeNodesetModelResult.Value as NodeSetModel;
                var activeNodesetProject = (((ApplicationInstance.GetNodeSetProjectInstance(id)) as ObjectResult).Value as NodeSetProjectInstance).NodeSetModels;
                
                var exportedNodeSetXml = UANodeSetModelExporter.ExportNodeSetAsXml(activeNodesetModel, activeNodesetProject);


                var filePath = Path.GetTempFileName();

                System.IO.File.WriteAllText(filePath, exportedNodeSetXml);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                System.IO.File.Delete(filePath);

                // remove header line before load
                // string xmlNoHeaderLine = exportedNodeSetXml.Substring(exportedNodeSetXml.IndexOf(Environment.NewLine));

                // remove preamble
                // string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                // if (exportedNodeSetXml.StartsWith(_byteOrderMarkUtf8))
                // {
                //     exportedNodeSetXml = exportedNodeSetXml.Remove(0, _byteOrderMarkUtf8.Length);
                // }

                return Ok(xmlDoc);

            }
        }



    }
}
