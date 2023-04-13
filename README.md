# OPC UA NodeSet Utilities WebAPI

## Summary

This project allows loading, editing, creating and saving of OPC UA NodeSet information models. 
The work is targeted specifically for Information Models, as described in 
[OPC 10000-5: UA Part 5](https://reference.opcfoundation.org/Core/Part5/v104/docs/). It presents
a wrapper for the [CESMII NodeSet Utilities](https://github.com/cesmii/CESMII-NodeSet-Utilities) 
in form of a .NET Core Restful WebAPI, which can be hosted on any system that can run modern 
multi-platform .NET WebAPI's. It further allows us to interface with UA NodeSets from any platform 
that can make web requests, thus reducing the entrance barrier to programatically accessing and 
manipulating OPC UA NodeSets.

## Getting Started

The best way to explore the project is to take a look at the 
[Swagger](https://opcuanodesetwebapi.azurewebsites.net/swagger/index.html) api documentation.

### UA NodeSet Projects

The entrance point into the api is the notion of a project. Projects are stored in memory and present 
the context of where multiple NodeSet namespaces are loaded, resolved, and offered for exploration and
modification. To create a project, all that is needed is a name - the system will provide a unique id
that can be used to work with the project. Projects contain a log to be able to review certain transactions.
Projects are deleted simply by providing the id to the delete route.

- GET /NodesetProject: to obtain all existing projects
- PUT /NodesetProject: to create a new project
- GET /NodesetProject/id: to obtain a project by id
- DELETE /NodesetProject/id: to delete a project by id

### UA NodeSet Models

Once we have a project, we probably want to include a few nodesets to work off of. At the very least 
we would want to load what is commonly refered to as namespace zero: http://opcfoundation.org/UA/. Nodesets 
can only be loaded if all required resources are in place - so the sequence of loading nodeset files
is important. Nodesets can be loaded from the server, if available, or by uploading a nodeset file.

- GET /NodesetProject/id/NodesetModel: to obtain all nodesets for a project
- PUT /NodesetProject/id/NodesetModel: to create a new nodeset in a project
- POST /NodesetProject/id/NodesetModel/LoadNodesetXmlFromServerAsync: to load a nodeset from the server
- POST /NodesetProject/id/NodesetModel/UploadNodesetXmlFromFileAsync: to load a nodeset by uploading a file

*Note: When nodesets are added to a project we use the namespace uri as a key for further drill down. Because 
Azure web app's don't do slashes well in url's of resource-style web api's, we remove the slashes.*

> Roadmap: Be able to view, add, and remove nodeset files on the server
>
> Roadmap: Be able to load a nodeset from a cloud library by namespace
>
> Roadmap: Be able to automatically resolve namespace dependencies from a cloud library

### Object Types

We can manage object types in a nodeset.

- GET /NodesetProject/id/NodesetModel/uri/ObjectType: to obtain all object types in a nodeset

### Properties

We can manage properties in a nodeset.

*Note: Properties are flat-listed for the whole nodeset. To contextualize which node a property
belongs to, use the parent node id, which could point to a data variable or an object type, for instance.*

- GET /NodesetProject/id/NodesetModel/uri/Property: to obtain all object types in a nodeset
- GET /NodesetProject/id/NodesetModel/uri/Property/nodeId: to obtain an object types in a nodeset by node id


### Data Variables

We can manage data variables in a nodeset.

*Note: Data variables are flat-listed for the whole nodeset. To contextualize which node a data variable
belongs to, use the parent node id, which could point to an object type, for instance.*

- GET /NodesetProject/id/NodesetModel/uri/DataVariable: to obtain all object types in a nodeset
