using OPC_UA_Nodeset_WebAPI.Model;
using System.Collections.Concurrent;

namespace OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities
{
    public class ApplicationInstance
    {
        public ConcurrentDictionary<string, NodeSetProjectInstance> NodeSetProjectInstances { get; } = new ConcurrentDictionary<string, NodeSetProjectInstance>();

    }
}
