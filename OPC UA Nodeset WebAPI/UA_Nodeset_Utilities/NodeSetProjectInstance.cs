using CESMII.OpcUa.NodeSetModel.Factory.Opc;
using CESMII.OpcUa.NodeSetModel;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;
using Opc.Ua;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities
{
    public class NodeSetProjectInstance
    {
        const string strNodeSetDirectory = "NodeSets";

        public string Name { get; set; }
        public string Owner { get; set; }
        public string ProjectId { get; set; }

        public Dictionary<string, string> Log { get; set; }

        public Dictionary<string, NodeSetModel> NodeSetModels { get; set; }
        public Dictionary<string, int> NextNodeIds { get; set; }

        public NodeSetModel UaBaseModel
        {
            get
            {
                if (NodeSetModels.ContainsKey("http://opcfoundation.org/UA/"))
                {
                    return NodeSetModels["http://opcfoundation.org/UA/"];
                }
                else
                {
                    return null;
                }
            }
        }

        public DefaultOpcUaContext opcContext { get; set; }

        UANodeSetModelImporter importer { get; set; }

        public NodeSetProjectInstance(string name, string owner, string projectId = "")
        {
            ProjectId = projectId;
            Name = name;
            Owner = owner;
            importer = new UANodeSetModelImporter(NullLogger.Instance);
            NodeSetModels = new();
            NextNodeIds = new();
            opcContext = new DefaultOpcUaContext(NodeSetModels, NullLogger.Instance);
            Log = new();
        }

        public async Task<string> LoadNodeSetFromFileOnServerAsync(string name)
        {
            var file = Path.Combine(strNodeSetDirectory, name);
            UANodeSet nodeSet;
            try
            {
                var fileStream = new FileStream(file, FileMode.Open);
                nodeSet = UANodeSet.Read(fileStream);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Error: File {name} did not work. {e.Message}");
            }
            ModelTableEntry modelEntry = nodeSet.Models.FirstOrDefault();

            return await TryImportOfNodeset(nodeSet, modelEntry);
        }

        public async Task<string> LoadNodeSetFromFileUploadAsync(string xml)
        {
            UANodeSet nodeSet;
            try
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(xml);
                MemoryStream stream = new MemoryStream(byteArray);

                nodeSet = UANodeSet.Read(stream);
            }
            catch (Exception e)
            {
                return "Error: File did not work.";
            }

            ModelTableEntry modelEntry = nodeSet.Models.FirstOrDefault();

            return await TryImportOfNodeset(nodeSet, modelEntry);
        }

        private async Task<string> TryImportOfNodeset(UANodeSet nodeSet, ModelTableEntry modelEntry)
        {
            // check if namespace is already present
            if (NodeSetModels.Where(x => x.Value.ModelUri == modelEntry.ModelUri).Count() > 0)
            {
                throw new InvalidOperationException($"Error: NodeSet {modelEntry.ModelUri} already exists.");
            }

            // check if all requirements are in place
            if (modelEntry.RequiredModel != null)
            {
                foreach (var aRequiredModelUri in modelEntry.RequiredModel.Select(x => x.ModelUri))
                {
                    if (!NodeSetModels.ContainsKey(aRequiredModelUri))
                    {
                        throw new InvalidOperationException($"Error: NodeSet can not be imported: {aRequiredModelUri} required.");
                    }
                }
            }

            // attempt import of nodeset
            try
            {
                await importer.LoadNodeSetModelAsync(opcContext, nodeSet);

                NextNodeIds.Add(modelEntry.ModelUri, 10000);

                return modelEntry.ModelUri;
            }
            catch (Exception e)
            {
                Log.Add(modelEntry.ModelUri, e.Message);
                throw new InvalidOperationException($"Error: NodeSet can not be imported: {e.Message}");
            }
        }

        public string AddNewNodeSet(string aModelUri)
        {
            NodeSetModel uaBaseModel;
            if (NodeSetModels.ContainsKey(Namespaces.OpcUa))
            {
                uaBaseModel = NodeSetModels[Namespaces.OpcUa];
            }
            else
            {
                return "Error: OPC UA Nodeset required.";
            }

            var newNodeSetModel = new NodeSetModel
            {
                ModelUri = aModelUri,
                RequiredModels = new List<RequiredModelInfo>
                {
                    new RequiredModelInfo { ModelUri= uaBaseModel.ModelUri, PublicationDate = uaBaseModel.PublicationDate, Version = uaBaseModel.Version}
                },
            };

            NodeSetModels.Add(newNodeSetModel.ModelUri, newNodeSetModel);

            NextNodeIds.Add(newNodeSetModel.ModelUri, 10000);

            return newNodeSetModel.ModelUri;

        }

        /**
         * Remove a nodeset from the project
         *
         * @param aModelUri the URI of the nodeset to remove
         */
        public string RemoveNodeSet(string aModelUri)
        {
            if (NodeSetModels.ContainsKey(aModelUri))
            {
                // remove all nodes from nodeset
                NodeSetModels.Remove(aModelUri);

                // remove all references to nodeset
                NextNodeIds.Remove(aModelUri);

                return aModelUri;
            }
            return "Error: Nodeset not found.";
        }

        public NodeModel GetNodeModelByNodeId(string nodeId)
        {
            var nodeFromNodeId = new UaNodeResponse { NodeId = nodeId };
            var modelEntry = NodeSetModels.FirstOrDefault(x => x.Value.ModelUri == nodeFromNodeId.NameSpace);

            if (modelEntry.Value != null && modelEntry.Value.AllNodesByNodeId.ContainsKey(nodeId))
            {
                var node = modelEntry.Value.AllNodesByNodeId[nodeId];
                return node;
            }

            throw new InvalidOperationException($"Node with ID {nodeId} not found.");
        }

        //public ObjectTypeModel GetObjectTypeModelByNodeId(string nodeId)
        //{
        //    //var id = int.Parse(nodeId.Split("=").Last());
        //    //var uri = nodeId.Split("=")[1].Split(";").First();

        //    //var nodeset = NodeSetModels[uri];
        //    //var objectType = nodeset.ObjectTypes.FirstOrDefault(x => x.NodeId == nodeId);

        //    var aNode = GetNodeModelByNodeId(nodeId);
        //    var aObjectTypeModel = aNode as ObjectTypeModel;

        //    return aObjectTypeModel;

        //}
        //public DataTypeModel GetDataTypeModelByNodeId(string nodeId)
        //{
        //    //var id = int.Parse(nodeId.Split("=").Last());
        //    //var uri = nodeId.Split("=")[1].Split(";").First();

        //    //var nodeset = NodeSetModels[uri];
        //    //var dataType = nodeset.DataTypes.FirstOrDefault(x => x.NodeId == nodeId);

        //    var aNode = GetNodeModelByNodeId(nodeId);
        //    var aDataTypeModel = aNode as DataTypeModel;

        //    return aDataTypeModel;

        //}
        //public VariableTypeModel GetVariableTypeModelByNodeId(string nodeId)
        //{
        //    var id = int.Parse(nodeId.Split("=").Last());
        //    var uri = nodeId.Split("=")[1].Split(";").First();

        //    var nodeset = NodeSetModels[uri];
        //    var variableType = nodeset.VariableTypes.FirstOrDefault(x => x.NodeId == nodeId);

        //    return variableType;

        //}
    }
}
