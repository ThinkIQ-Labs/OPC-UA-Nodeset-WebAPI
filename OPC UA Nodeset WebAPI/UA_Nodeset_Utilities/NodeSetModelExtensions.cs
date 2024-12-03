using CESMII.OpcUa.NodeSetModel;

namespace OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities
{
    public static class NodeSetModelExtensions
    {

        public static IEnumerable<ObjectModel> GetObjects(this NodeSetModel nodeSetModel)
        {
            return nodeSetModel.AllNodesByNodeId.Where(x => x.Value.GetType().IsEquivalentTo(typeof(ObjectModel))).Select(x => (ObjectModel)x.Value);
        }

        public static IEnumerable<PropertyModel> GetProperties(this NodeSetModel nodeSetModel)
        {
            return nodeSetModel.AllNodesByNodeId.Where(x => x.Value.GetType().IsEquivalentTo(typeof(PropertyModel))).Select(x => (PropertyModel)x.Value);
        }

        public static IEnumerable<DataVariableModel> GetDataVariables(this NodeSetModel nodeSetModel)
        {
            return nodeSetModel.AllNodesByNodeId.Where(x => x.Value.GetType().IsEquivalentTo(typeof(DataVariableModel))).Select(x => (DataVariableModel)x.Value);
        }

    }
}
