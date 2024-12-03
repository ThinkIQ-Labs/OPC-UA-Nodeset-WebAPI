using static CESMII.OpcUa.NodeSetModel.NodeModel;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiNodeAndReferenceModel
    {
        public string NodeId { get; set; }
        public string NodeDisplayName { get; set; }

        public string ReferenceType { get; set; }
        public string ReferenceTypeName { get; set; }
        public ApiNodeAndReferenceModel() { }
        public ApiNodeAndReferenceModel(NodeAndReference aNodeAndReference)
        {
            NodeId = aNodeAndReference.Node.NodeId;
            NodeDisplayName = aNodeAndReference.Node.DisplayName.First().Text;
            ReferenceType = aNodeAndReference.ReferenceType?.NodeId;
            ReferenceTypeName = aNodeAndReference.ReferenceType?.DisplayName.First().Text;
        }
    }
}
