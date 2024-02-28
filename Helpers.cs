using System.Collections.Concurrent;
using System.Text;


namespace Nimbus;

public static class NET8_Helpers
{
    public static readonly Dictionary<string, string> typeQualifiers = new()
    {
        {"System.Private.CoreLib", ", mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"},
        {"System.Private.Uri", ", System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"}
    };
    public static readonly ConcurrentDictionary<Type, string> typeLookups = new();



    public static string GetLegacy(this Type type, bool qualify = false, StringBuilder? builder = null)
    {   
        StringBuilder sb = builder ?? new();

        
        if (type.Namespace != null)
        {
            sb.Append(type.Namespace);
            sb.Append('.');
        }


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
        else
        {
            sb.Append(type.Name);
        }


        if (type.IsGenericTypeDefinition)
            return sb.ToString();


        if (type.IsGenericType)
        {
            sb.Append('[');
            foreach (Type arg in type.GenericTypeArguments)
            {
                sb.Append('[');
                arg.GetLegacy(true, sb);
                sb.Append(']');
                sb.Append(',');
            }
            sb.Length--;
            sb.Append(']');
        }


        if (qualify)
        {
            if (typeQualifiers.TryGetValue(type.Assembly.GetName().Name!, out string? qualifier))
            {
                sb.Append(qualifier);
            }
            else
            {
                sb.Append(", " + type.Assembly.FullName);
            }
        }
    
        return sb.ToString();
    }



    public static string TryGetLegacy(this Type type)
    {
        if (typeLookups.TryGetValue(type, out string typeName))
        {
            Nimbus.Debug($"Got cache hit for {type.FullName}! Legacy is: {typeName}!");
            return typeName;
        }
        else
        {
            string legacyName = type.GetLegacy();
            typeLookups.TryAdd(type, legacyName);
            return legacyName;
        }
    }
}