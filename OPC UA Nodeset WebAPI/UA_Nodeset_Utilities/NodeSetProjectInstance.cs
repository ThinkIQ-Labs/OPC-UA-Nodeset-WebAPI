﻿using CESMII.OpcUa.NodeSetModel.Factory.Opc;
using CESMII.OpcUa.NodeSetModel;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Ua.Export;
using OPC_UA_Nodeset_WebAPI.Model;
using Opc.Ua;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace OPC_UA_Nodeset_WebAPI.UA_Nodeset_Utilities
{
    public class NodeSetProjectInstance
    {
        const string strNodeSetDirectory = "NodeSets";

        public string Name { get; set; }

        public Dictionary<string, string> Log { get; set; }

        public Dictionary<string, NodeSetModel> NodeSetModels { get; set; }

        DefaultOpcUaContext opcContext { get; set; }

        UANodeSetModelImporter importer { get; set; }

        public NodeSetProjectInstance(string name)
        {
            Name = name;
            importer = new UANodeSetModelImporter(NullLogger.Instance);
            NodeSetModels = new();
            opcContext = new DefaultOpcUaContext(NodeSetModels, NullLogger.Instance);
            Log = new();
        }

        public async Task<string> LoadNodeSetFromFileOnServerAsync(string name)
        {
            var file = Path.Combine(strNodeSetDirectory, name);
            UANodeSet nodeSet;
            try
            {
                nodeSet = UANodeSet.Read(new FileStream(file, FileMode.Open));
            }
            catch (Exception e)
            {
                return "Error: File not found.";
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
            } catch(Exception e)
            {
                return "Error: File did not work.";
            }

            ModelTableEntry modelEntry = nodeSet.Models.FirstOrDefault();

            return await TryImportOfNodeset(nodeSet, modelEntry);
        }
        private async Task<string> TryImportOfNodeset(UANodeSet nodeSet, ModelTableEntry modelEntry)
        {
            bool allowImport = true;
            if (modelEntry.RequiredModel != null)
            {
                foreach (var aRequiredModelUri in modelEntry.RequiredModel.Select(x => x.ModelUri))
                {
                    if (!NodeSetModels.ContainsKey(aRequiredModelUri))
                    {
                        allowImport = false;
                        return $"Error: NodeSet can not be imported. {aRequiredModelUri} required.";
                    }
                }
            }
            if (allowImport)
            {
                await importer.LoadNodeSetModelAsync(opcContext, nodeSet);
                return modelEntry.ModelUri;
            }
            else
            {
                return "Error: NodeSet can not be imported.";
            }
        }
        public string AddNewNodeSet(string domain, string name)
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
                ModelUri = $"{domain}{(domain.EndsWith("/") ? "" : "/")}{name}{(name.EndsWith("/") ? "" : "/")}",
                RequiredModels = new List<RequiredModelInfo>
                {
                    new RequiredModelInfo { ModelUri= uaBaseModel.ModelUri, PublicationDate = uaBaseModel.PublicationDate, Version = uaBaseModel.Version}
                },
            };

            NodeSetModels.Add(newNodeSetModel.ModelUri, newNodeSetModel);
            
            return newNodeSetModel.ModelUri;
            
        }
    }
}
