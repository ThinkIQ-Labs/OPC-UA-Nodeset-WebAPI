using CESMII.OpcUa.NodeSetModel;
using Microsoft.AspNetCore.Mvc;
using OPC_UA_Nodeset_WebAPI.Model;
using System.Collections.Concurrent;
using System.Web;

namespace OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities
{
    public class ApplicationInstance : ControllerBase
    {
        public ConcurrentDictionary<string, NodeSetProjectInstance> NodeSetProjectInstances { get; } = new ConcurrentDictionary<string, NodeSetProjectInstance>();

        public IActionResult GetNodeSetProjectInstance(string id)
        {
            NodeSetProjectInstance aNodesetProjectInstance;
            if (NodeSetProjectInstances.TryGetValue(id, out aNodesetProjectInstance))
            {
                return Ok(aNodesetProjectInstance);
            }
            else
            {
                return NotFound($"{id} - not a valid project id."); ; // because the project doesn't exist
            }
        }

        public IActionResult GetNodeSetModel(string id, string uri)
        {
            //var uriNoSlashes = HttpUtility.UrlDecode(uri); ;
            var uriNoSlashes = HttpUtility.UrlDecode(uri).Replace("/", "");
            NodeSetProjectInstance aNodesetProjectInstance;
            if (NodeSetProjectInstances.TryGetValue(id, out aNodesetProjectInstance))
            {
                NodeSetModel aNodesetModel;
                //if (aNodesetProjectInstance.NodeSetModels.Keys.Contains(uriNoSlashes))
                if (aNodesetProjectInstance.NodeSetModels.Keys.Select(x => x.Replace("/", "")).Contains(uriNoSlashes))
                {
                    //return Ok(aNodesetProjectInstance.NodeSetModels.First(x => x.Value.ModelUri == uriNoSlashes).Value);
                    return Ok(aNodesetProjectInstance.NodeSetModels.First(x => x.Value.ModelUri.Replace("/", "") == uriNoSlashes).Value);
                }
                else
                {
                    return NotFound("The model does not exist.");
                }
            }
            else
            {
                return NotFound("The project does not exist.");
            }
        }

    }
}
