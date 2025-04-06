using System.Text;
using System.Text.Json;

namespace AppSettingsClass.Core.Generators;


public class AppSettingsDefinitionsGenerator
{
    private const string _divider = "    //- - - - - - - - - - - - - - - -//        ";


    public static string GenerateDefinitionsClass(string jsonContent, string namespaceName)
    {
        // Remove commented-out lines
        var filteredJsonContent = RemoveCommentsFromJson(jsonContent);
        string topLevelIndent = new(' ', 4);
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("#pragma warning disable CA2211 // Non-constant fields should not be visible");
        sb.AppendLine("public class AppSettingsDefinitions");
        sb.AppendLine("{");

        using (JsonDocument doc = JsonDocument.Parse(filteredJsonContent))
        {
            foreach (var element in doc.RootElement.EnumerateObject())
            {
                if (element.Value.ValueKind == JsonValueKind.Object)
                    sb.AppendLine(GenerateClass(element, 1));
                else
                    sb.AppendLine(GenerateClassPropertyPair(element, topLevelIndent));
            }
        }

        sb.AppendLine("}");
        sb.AppendLine("#pragma warning restore CA2211 // Non-constant fields should not be visible");

        return sb.ToString();
    }

    //---------------------------------//

    private static string GenerateClass(JsonProperty element, int indentLevel)
    {
        var sb = new StringBuilder();
        string indent = new(' ', indentLevel * 4);
        string className = element.Name + "Section";

        sb.AppendLine($"{indent}");
        sb.AppendLine($"{indent}{_divider}");
        sb.AppendLine($"{indent}");

        sb.AppendLine($"{indent}public static class {className}");
        sb.AppendLine($"{indent}{{");
        sb.AppendLine($"{indent}    public const string Name = \"{element.Name}\";");

        foreach (var property in element.Value.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
                sb.AppendLine(GenerateClass(property, indentLevel + 1));
            else
                sb.AppendLine($"{GenerateClassPropertyPair(property, indent)}");
        }

        sb.AppendLine($"{indent}}}");

        return sb.ToString();
    }

    //---------------------------------//

    private static string GenerateClassPropertyPair(JsonProperty property, string indent)
    {
        var sb = new StringBuilder();
        string keyName = property.Name + "Key";
        string typeName = GetTypeName(property);
        string typePropertyName = property.Name + "Type";

        sb.AppendLine($"{indent}");
        sb.AppendLine($"{indent}    public const string {keyName} = \"{property.Name}\";");
        sb.AppendLine($"{indent}    public static Type {typePropertyName} = typeof({typeName});");

        return sb.ToString();

    }

    //---------------------------------//

    private static string GetTypeName(JsonProperty property)
    {
        if(IsLogLevel(property))
            return "LogLevel";

        var element = property.Value;
        return element.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => element.TryGetInt32(out _) ? "int" : "double", //Try int first becauxe double will catch int
            JsonValueKind.True => "bool",
            JsonValueKind.False => "bool",
            JsonValueKind.Array => GetArrayTypeName(element),
            _ => "object",
        };
    }

    //---------------------------------//

    private static string GetArrayTypeName(JsonElement arrayElement)
    {
        if (arrayElement.GetArrayLength() == 0)
            return "string[]";

        var firstElement = arrayElement[0];
        return firstElement.ValueKind switch
        {
            JsonValueKind.String => "string[]",
            JsonValueKind.Number => firstElement.TryGetInt32(out _) ? "int[]" : "double[]",//Try int first becauxe double will catch int
            JsonValueKind.True => "bool[]",
            JsonValueKind.False => "bool[]",
            _ => "string[]",
        };
    }

    //---------------------------------//

    private static string RemoveCommentsFromJson(string jsonContent)
    {
        var lines = jsonContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("//"));
        return string.Join(Environment.NewLine, filteredLines);
    }

    //---------------------------------//
    internal static bool IsLogLevel(JsonProperty property) =>
        property.Name.Contains("loglevel", StringComparison.CurrentCultureIgnoreCase);

}//Cls