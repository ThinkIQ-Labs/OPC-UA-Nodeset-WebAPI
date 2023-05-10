namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiUaNodeModel
    {
        public static string GetNodeIdFromIdAndNameSpace(uint id, string ns)
        {
            //nsu=http://opcfoundation.org/UA/;i=58
            return $"nsu={ns};i={id}";
        }
        public string? NodeId { get; set; }

        public static uint GetIdFromNodeId(string nodeId)
        {
                if (nodeId == null) return 0;
                if (!nodeId.Contains("=")) return 0;
                return uint.Parse(nodeId.Split("=").Last());
        }
        public uint Id
        {
            get
            {
                return GetIdFromNodeId(NodeId);
            }
        }

        public static string GetNameSpaceFromNodeId(string nodeId)
        {
                if (nodeId == null) return "";
                if (!nodeId.Contains("=") || !nodeId.Contains(";")) return "";
                return nodeId.Split("=")[1].Split(";").First();

        }
        public string NameSpace
        {
            get
            {
                return GetNameSpaceFromNodeId(NodeId);
            }
        }

        public string? DisplayName { get; set; }
        public string? BrowseName { get; set; }

        public string? Description { get; set; }

    }
}
