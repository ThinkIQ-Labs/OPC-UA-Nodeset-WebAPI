using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.Model.v1;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Xml;

// @TODO: extract some of the repeated logic into a helper method
namespace OPC_UA_Nodeset_WebAPI.api.v1.Controllers
{
    [ApiController]
    [Route("api/v1/local-nodeset")]
    public class LocalNodesetController : ControllerBase
    {
        private readonly ILogger<NodesetModelController> _logger;

        private ApplicationInstance ApplicationInstance { get; set; }

        public LocalNodesetController(ILogger<NodesetModelController> logger, ApplicationInstance applicationInstance)
        {
            _logger = logger;
            ApplicationInstance = applicationInstance;
        }

        /// <summary>
        /// Returns the nodesets available locally on the server
        /// including file name, namespace, version, publish date, and all dependencies.
        /// </summary>
        /// <returns>Returns a dictionary with nodeset metadata where the file name is used as key.</returns>
        /// <response code="200">The nodesets locally available were successfully listed.</response>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(ConcurrentDictionary<string, ApiNodeSetInfoWithDependencies>))]
        public IActionResult ListNodesetsOnServer()
        {
            return Ok(ApplicationInstance.LocalNodesets);
        }

        /// <summary>
        /// Returns a nodeset xml file available locally on the server
        /// </summary>
        /// <returns>Returns a nodeset xml file available locally on the server.</returns>
        /// <response code="200">The nodesets locally available were successfully delivered.</response>
        [HttpGet("{fileName}")]
        [Produces("application/xml")]
        [ProducesResponseType(200, Type = typeof(ConcurrentDictionary<string, ApiNodeSetInfoWithDependencies>))]
        public IActionResult LocalNodesetXML(string fileName)
        {
            var filePath = $"{AppContext.BaseDirectory}/NodeSets/{fileName}";
            if (System.IO.File.Exists(filePath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);
                return Ok(xmlDoc);
            }
            return NotFound($"{fileName} was not found on the server.");
        }

        /// <summary>
        /// Deletes a nodeset file locally on the server.
        /// </summary>
        /// <returns>Returns a dictionary with nodeset metadata where the file name is used as key.</returns>
        /// <response code="200">The nodeset was successfully deleted from the server.</response>
        /// <response code="404">The nodeset was not found on the server.</response>
        [HttpDelete("{fileName}")]
        [ProducesResponseType(200, Type = typeof(ConcurrentDictionary<string, ApiNodeSetInfoWithDependencies>))]
        [ProducesResponseType(404, Type = typeof(NotFoundResult))]
        public IActionResult Destroy(string fileName)
        {
            var uri = $"{AppContext.BaseDirectory}/NodeSets/{fileName}";
            if (System.IO.File.Exists(uri))
            {
                System.IO.File.Delete(uri);
                ApplicationInstance.ScanNodesetFiles();
                return Ok(ApplicationInstance.LocalNodesets);
            }
            return NotFound($"{fileName} was not found on the server.");
        }

        /// <summary>
        /// Loads a nodeset file from a file upload.
        /// </summary>
        /// <param name="file"></param>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        //https://medium.com/@niteshsinghal85/testing-file-upload-with-swagger-in-asp-net-core-90269bc24fe8
        [HttpPut("upload-xml-from-file-async")]
        [ProducesResponseType(200, Type = typeof(ApiNodeSetInfoWithDependencies))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        public async Task<IActionResult> UploadNodesetXmlFromFileAsync(IFormFile file)
        {
            var filePath = $"{AppContext.BaseDirectory}/NodeSets/{file.FileName}";

            try
            {
                // save the file from a stream
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                // we saved a file. now we need to verify it's a legit nodeset
                string aXmlString = System.IO.File.ReadAllText(filePath);
                var aNodeSet = UANodeSetFromString.Read(aXmlString);
                var returnObject = new ApiNodeSetInfoWithDependencies(aNodeSet);

                // since we added a nodeset we have to update the listing of all available nodesets
                ApplicationInstance.ScanNodesetFiles();

                return Ok(returnObject);
            }
            catch (Exception ex)
            {
                // if we're here something went wrong and we need to delete the file
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return BadRequest($"{file.FileName} could not be added to the server. Is it a valid nodeset xml file?");
            }
        }

        /// <summary>
        /// Loads a nodeset file from a string that is encoded using base64.
        /// </summary>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        //https://medium.com/@niteshsinghal85/testing-file-upload-with-swagger-in-asp-net-core-90269bc24fe8
        [HttpPost("upload-xml-from-base-64")]
        [ProducesResponseType(200, Type = typeof(ApiNodeSetInfoWithDependencies))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        public async Task<IActionResult> UploadNodesetXmlFromBase64([FromBody] UANodeSetBase64Upload request)
        {
            var fileName = request.FileName;
            var xmlContent = request.XmlBase64;
            var filePath = $"{AppContext.BaseDirectory}/NodeSets/{fileName}";

            try
            {
                var valueBytes = Convert.FromBase64String(xmlContent);
                var xml = Encoding.UTF8.GetString(valueBytes);

                System.IO.File.WriteAllText(filePath, xml);

                // we saved a file. now we need to verify it's a legit nodeset
                string aXmlString = System.IO.File.ReadAllText(filePath);
                var aNodeSet = UANodeSetFromString.Read(aXmlString);
                var returnObject = new ApiNodeSetInfoWithDependencies(aNodeSet);

                // since we added a nodeset we have to update the listing of all available nodesets
                ApplicationInstance.ScanNodesetFiles();

                return Ok(returnObject);
            }
            catch (Exception ex)
            {
                // if we're here something went wrong and we need to delete the file
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return BadRequest($"{fileName} could not be added to the server. Is it a valid nodeset xml file?");

            }
        }

        /// <summary>
        /// Loads a nodeset file from a string that is encoded using base64.
        /// </summary>
        /// <returns>Returns a loaded nodeset model for a nodeset project.</returns>
        /// <response code="200">The nodeset was successfully loaded and parsed as a nodeset model.</response>
        /// <response code="400">The nodeset could not be loaded.</response>
        //https://medium.com/@niteshsinghal85/testing-file-upload-with-swagger-in-asp-net-core-90269bc24fe8
        [HttpPost("get-info-xml-from-base-64")]
        [ProducesResponseType(200, Type = typeof(ApiNodeSetInfoWithDependencies))]
        [ProducesResponseType(400, Type = typeof(BadRequestResult))]
        public async Task<IActionResult> GetInfoNodesetXmlFromBase64([FromBody] UANodeSetBase64Upload request)
        {
            var fileName = request.FileName;
            var xmlContent = request.XmlBase64;
            var filePath = Path.GetTempFileName();

            try
            {
                var valueBytes = Convert.FromBase64String(xmlContent);
                var xml = Encoding.UTF8.GetString(valueBytes);

                System.IO.File.WriteAllText(filePath, xml);

                // we saved a file. now we need to verify it's a legit nodeset
                string aXmlString = System.IO.File.ReadAllText(filePath);
                var aNodeSet = UANodeSetFromString.Read(aXmlString);
                var returnObject = new ApiNodeSetInfoWithDependencies(aNodeSet);

                System.IO.File.Delete(filePath);

                return Ok(returnObject);
            }
            catch (Exception ex)
            {
                // if we're here something went wrong and we need to delete the file
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                return BadRequest($"{fileName} could not be parsed. Is it a valid nodeset xml file?");
            }
        }
    }
}
