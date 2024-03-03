using System.Collections.Concurrent;
using System.Text;


namespace Nimbus;

/// <summary>
/// Helpers for .NET 8 Compatibility
/// </summary>
public static class NET8_Helpers
{
    // TODO: Make this more universal
    /// <summary>
    /// New namespaces mapped to old qualifiers
    /// </summary>
    public static readonly Dictionary<string, string> AssemblyQualifiers = new()
    {
        {"System.Private.CoreLib", ", mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"},
        {"System.Private.Uri", ", System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"},
        {"System.Net.Primitives", ", System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"}
    };

    /// <summary>
    /// New types mapped to their legacy FullName
    /// </summary>
    public static readonly ConcurrentDictionary<Type, string> TypeLookups = new();



    private static string GetLegacyInternal(this Type type, bool qualify = false, StringBuilder? builder = null)
    {   
        StringBuilder sb = builder ?? new(); // Use the stringbuilder from the top-level call, or a new one if none exists
        
        // Append namespace if one exists
        if (type.Namespace != null)
        {
            sb.Append(type.Namespace);
            sb.Append('.');
        }

        // If the type is nested, walk up the parent types to the top and append them to the string builder (e.g. GradientSkyMaterial+Gradient)
        if (type.IsNested)
        {
            Type nestedCheck = type;
            StringBuilder nestBuilder = new();
            nestBuilder.Insert(0, type.Name);
            while (nestedCheck.IsNested)
            {
                Type declaring = nestedCheck.DeclaringType!;
                nestBuilder.Insert(0, '+');
                nestBuilder.Insert(0, declaring.Name);
                nestedCheck = declaring;
            }
            sb.Append(nestBuilder);
        }
        else // Otherwise just append the type name
        {
            sb.Append(type.Name);
        }

        // If the type is itself a generic type definition without any parameters defined, stop here and return the type name
        if (type.IsGenericTypeDefinition)
            return sb.ToString();

        // If it's a generic type with arguments, add brackets to conform to the proper formatting (e.g. FrooxEngine.ValueField`1[[System.Single, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]])
        if (type.IsGenericType)
        {
            sb.Append('[');
            foreach (Type arg in type.GenericTypeArguments)
            {
                sb.Append('[');
                arg.GetLegacyInternal(true, sb); // Get the legacy assembly qualifier for the type
                sb.Append(']');
                sb.Append(',');
            }
            sb.Length--;
            sb.Append(']');
        }

        // If the type is to be qualified, get the legacy assembly qualifier (e.g. ", mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
        if (qualify)
        {
            // If the type's assembly has a legacy qualifier, use that
            if (AssemblyQualifiers.TryGetValue(type.Assembly.GetName().Name!, out string? qualifier))
            {
                sb.Append(qualifier);
            }
            else // Otherwise just use the type's normal assembly qualifier
            {
                sb.Append(", " + type.Assembly.FullName);
            }
        }
    
        return sb.ToString();
    }


    /// <summary>
    /// Gets the legacy qualifier for types as they were .NET Framework
    /// </summary>
    /// <param name="type">The type to get the legacy FullName of</param>
    /// <returns></returns>
    public static string GetLegacy(this Type type)
    {
        // Check the cache first, then construct one if it's not in the cache yet
        if (TypeLookups.TryGetValue(type, out string typeName))
        {
            // Nimbus.Debug($"Got cache hit for {type.FullName}! Legacy is: {typeName}!");
            return typeName;
        }
        else
        {
            string legacyName = type.GetLegacyInternal();
            TypeLookups.TryAdd(type, legacyName);
            return legacyName;
        }
    }
}