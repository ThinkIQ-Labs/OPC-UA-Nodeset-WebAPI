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
    /// <exception cref="Exception">Thrown if an object type with the same display name already exists.</exception>
    /// <returns>void</returns>
    protected void FindOpcType<T>(List<T> opcTypes, dynamic request) where T : UaNodeResponse
    {
        T? found;
        var rawName = typeof(T).Name.Replace("Response", "");
        var hashSet = new HashSet<string>(){
            "DataVariable",
            "ObjectModel",
            "Property"
        };
        Console.WriteLine($"BaseType FullName: {typeof(T).BaseType?.FullName}");
        Console.WriteLine($"RawName: {rawName}");
        Console.WriteLine($"DisplayName: {request.DisplayName}");
        foreach (var type in opcTypes)
        {
            Console.WriteLine($"type: {type.ParentNodeId} == request: {request.ParentNodeId}");
            if (type.ParentNodeId == request.ParentNodeId)
            {
                Console.WriteLine($"Found ParentNodeId: {type.ParentNodeId}");
                if (type.DisplayName == request.DisplayName)
                {
                    Console.WriteLine($"Found Type: {type.DisplayName}");
                }
            }
        }


        if (hashSet.Contains(rawName))
        {
            found = opcTypes
                .Where(x => x.ParentNodeId == request.ParentNodeId)
                .FirstOrDefault(x => x.DisplayName == request.DisplayName);
        }
        else
        {
            found = opcTypes.FirstOrDefault(x => x.DisplayName == request.DisplayName);
        }

        if (found != null)
        {
            var typeName = Regex.Replace(rawName, "([a-z])([A-Z])", "$1 $2");
            throw new Exception($"'{typeName}' with DisplayName '{request.DisplayName}' already exists.");
        }

    }
}
