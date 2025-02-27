using System.Xml.Serialization;
using System.Xml;
using System.Reflection.PortableExecutable;
using OPC_UA_Nodeset_WebAPI.Model.v1;

namespace Opc.Ua.Export.v1
{
    public static class UANodeSetFromString
    {
        /// <summary>
        /// Loads a nodeset from a string.
        /// </summary>
        /// <param name="istr">The input string.</param>
        /// <returns>The set of nodes</returns>
        public static UANodeSet Read(String istr)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UANodeSet));
            using (TextReader reader = new StringReader(istr))
            {
                return serializer.Deserialize(reader) as UANodeSet;
            }
        }
    }

    public class UANodeSetBase64Upload
    {
        public string FileName { get; set; }
        public string XmlBase64 { get; set; }

        public UANodeSetBase64Upload() { }
    }

    public class UAEnumField
    {
        public string Name { get; set; }
        public string? DisplayName { get; set; }
        public string? Description { get; set; }
        public long Value { get; set; }

        public UAEnumField() { }
    }

    public class UAStructureField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DataTypeName { get; set; }
        public string DataTypeNodeId { get; set; }

        public UAStructureField() { }
    }

    /**
     * Request object for creating a new nodeset project.
     */
    public class NodesetRequest
    {
        public string name { get; set; }
        public string owner { get; set; }
    }

    public class NodesetFile
    {
        public string ProjectId { get; set; }
        public string? Uri { get; set; } = "";
        public IFormFile? File { get; set; }
        public string? FileName { get; set; }
        public string? XmlBase64 { get; set; }
        public ApiNodeSetInfo? apiNodeSetInfo { get; set; }
    }

    public class ApiCombinedResponse
    {
        public List<ApiObjectTypeModel> ObjectTypes { get; set; }
        public List<ApiDataVariableModel> DataVariables { get; set; }
        public List<ApiPropertyModel> Properties { get; set; }
        public List<ApiVariableTypeModel> VariableTypes { get; set; }
        public List<ApiDataTypeModel> DataTypes { get; set; }
        public List<ApiObjectModel> Objects { get; set; }
    }
}
