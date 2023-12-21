using CESMII.OpcUa.NodeSetModel;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities;
using System.Xml.Linq;

namespace OPC_UA_Nodeset_WebAPI.Model
{
    public class ApiDataTypeModel : ApiUaNodeModel
    {
        public string SuperTypeNodeId { get; set; }
        public List<UAEnumField> EnumFields { get; set; }
        public List<UAStructureField> StructureFields { get; set; }
        internal DataTypeModel? DataTypeModel { get; set; }
        public ApiDataTypeModel() { }

        public ApiDataTypeModel(DataTypeModel aDataTypeModel)
        {

            DataTypeModel = aDataTypeModel;
            NodeId = aDataTypeModel.NodeId;
            DisplayName = aDataTypeModel.DisplayName.First().Text;
            BrowseName = aDataTypeModel.BrowseName;
            Description = aDataTypeModel.Description.Count == 0 ? "" : aDataTypeModel.Description.First().Text;
            SuperTypeNodeId = aDataTypeModel.SuperType == null ? "" : aDataTypeModel.SuperType.NodeId;
            
            EnumFields = aDataTypeModel.EnumFields?.Select(x => new UAEnumField()
            {
                Description = x.Description.Count == 0 ? "" : x.Description.First().Text,
                DisplayName = x.DisplayName.Count == 0 ? "" : x.DisplayName.First().Text,
                Name = x.Name,
                Value = x.Value

            }).ToList();

            StructureFields = aDataTypeModel.StructureFields?.Select(x => new UAStructureField()
            {
                Name = x.Name,
                DataTypeNodeId = x.DataType.NodeId,
                DataTypeName = x.DataType.DisplayName.First().Text
            }).ToList();

        }

    }
}
