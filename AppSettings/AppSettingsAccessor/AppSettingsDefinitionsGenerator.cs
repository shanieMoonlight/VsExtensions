using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text;

namespace AppSettingsAccessorGeneration;


public class AppSettingsDefinitionsGenerator
{
    private const string _divider = "    //- - - - - - - - - - - - - - - -//        ";

    public static string GenerateDefinitionsClass(string jsonContent, string namespaceName, string debugMsg = null)
    {
        // Remove commented-out lines
        var filteredJsonContent = RemoveCommentsFromJson(jsonContent);
        string topLevelIndent = new(' ', 4);
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");  // First line
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"//{debugMsg}");
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("#pragma warning disable CA2211 // Non-constant fields should not be visible");
        sb.AppendLine("public class AppSettingsDefinitions");
        sb.AppendLine("{");

        var doc = JObject.Parse(filteredJsonContent);
        foreach (var element in doc.Properties())
        {
            if (element.Value.Type == JTokenType.Object)
                sb.AppendLine(GenerateClass(element, 1));
            else
                sb.AppendLine(GenerateClassPropertyPair(element, topLevelIndent));
        }

        sb.AppendLine("}");
        sb.AppendLine("#pragma warning restore CA2211 // Non-constant fields should not be visible");

        return sb.ToString();
    }

    //---------------------------------//

    /// <summary>
    /// Generates a class for each section in the JSON file.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="indentLevel"></param>
    /// <param name="isFromLogLevelClass">Tells the Generator to use LogLevelTypes for the props</param>
    /// <returns></returns>
    private static string GenerateClass(JProperty element, int indentLevel, bool isFromLogLevelClass = false)
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

        foreach (var property in element.Value.Children<JProperty>())
        {
            var isLogLevelClass = IsLogLevel(property);
            if (property.Value.Type == JTokenType.Object)
                sb.AppendLine(GenerateClass(property, indentLevel + 1, isLogLevelClass));
            else
                sb.AppendLine($"{GenerateClassPropertyPair(property, indent, isFromLogLevelClass)}");
        }

        sb.AppendLine($"{indent}}}");

        return sb.ToString();

    }

    //---------------------------------//

    private static string GenerateClassPropertyPair(JProperty property, string indent, bool isFromLogLevelClass = false)
    {
        var sb = new StringBuilder();
        string keyName = SanitizeName(property.Name + "Key");
        string typeName = isFromLogLevelClass ? "LogLevel" : GetTypeName(property);
        string typePropertyName = SanitizeName(property.Name + "Type");

        sb.AppendLine($"{indent}");
        sb.AppendLine($"{indent}    public const string {keyName} = \"{property.Name}\";");
        sb.AppendLine($"{indent}    public static Type {typePropertyName} = typeof({typeName});");

        return sb.ToString();
    }

    //---------------------------------//

    private static string GetTypeName(JProperty property)
    {
        if (IsLogLevel(property))
            return "LogLevel";

        var element = property.Value;
        return element.Type switch
        {
            JTokenType.String => "string",
            JTokenType.Integer => "int",
            JTokenType.Float => "double",
            JTokenType.Boolean => "bool",
            JTokenType.Array => GetArrayTypeName(element),
            _ => "object",
        };
    }

    //---------------------------------//

    private static string GetArrayTypeName(JToken arrayElement)
    {
        if (!arrayElement.HasValues)
            return "string[]";

        var firstElement = arrayElement.First;
        return firstElement.Type switch
        {
            JTokenType.String => "string[]",
            JTokenType.Integer => "int[]",
            JTokenType.Float => "double[]",
            JTokenType.Boolean => "bool[]",
            _ => "string[]",
        };
    }

    //---------------------------------//

    private static string RemoveCommentsFromJson(string jsonContent)
    {
        var lines = jsonContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("//"));
        return string.Join("\r\n", filteredLines);
    }

    //---------------------------------//

    private static string SanitizeName(string name)
    {
        return name.Replace(".", "_");
    }

    //---------------------------------//

    internal static bool IsLogLevel(JProperty property) =>
        property.Name.ToLowerInvariant().Contains("loglevel");

    //---------------------------------//



}//Cls


