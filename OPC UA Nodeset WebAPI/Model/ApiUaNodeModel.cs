namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiUaNodeModel
    {
        internal string? NodeId { get; set; }

        public int Id
        {
            get
            {
                if (NodeId == null) return 0;
                return int.Parse(NodeId.Split("=").Last());
            }
        }

        public string NameSpace
        {
            get
            {
                if (NodeId == null) return "";
                return NodeId.Split("=")[1].Split(";").First();
            }
        }

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

    }
}
