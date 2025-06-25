using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using OPC_UA_Nodeset_WebAPI.Model.v1.Responses;

public abstract class AbstractBaseController : ControllerBase
{
    /// <summary>
    /// Validates if an object type with the specified display name already exists in the provided list of OPC Types.
    /// Throws an exception if a duplicate is found.
    /// </summary>
    /// <typeparam name="T">The type of the OPC Type, which should inherit from UaNodeResponse.</typeparam>
    /// <param name="request">The request containing the display name to check.</param>
    /// <param name="type">The type instance to check against the list.</param>
    /// <exception cref="Exception">Thrown if an object type with the same display name already exists.</exception>
    /// <returns>void</returns>
    protected void FindOpcType<T>(List<T> opcTypes, dynamic request, dynamic type = null) where T : UaNodeResponse
    {
        T? found;
        var rawName = typeof(T).Name.Replace("Response", "");
        var hashSet = new HashSet<string>(){
            "DataVariable",
            "ObjectModel",
            "Property"
        };

        if (type != null && hashSet.Contains(rawName))
        {
            found = opcTypes
                .Where(x => x.ParentNodeId == request.ParentNodeId)
                .FirstOrDefault(x => x.BrowseName == type.BrowseName);

        }
        else if (hashSet.Contains(rawName))
        {
            found = opcTypes
                .Where(x => x.ParentNodeId == request.ParentNodeId)
                .FirstOrDefault(x => x.BrowseName == request.BrowseName);
        }
        else
        {
            found = opcTypes.FirstOrDefault(x => x.BrowseName == request.BrowseName);
        }

        if (found != null)
        {
            var typeName = Regex.Replace(rawName, "([a-z])([A-Z])", "$1 $2");
            throw new Exception($"'{typeName}' with BrowseName '{request.BrowseName}' already exists.");
        }

    }
}
