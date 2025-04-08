using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StronglyTypedAppSettings;


/// <summary>
/// Provides functionality to generate strongly-typed class definitions for application settings based on JSON content.
/// </summary>
public class AppSettingsDefinitionsGenerator
{
    private const string _divider = "    //- - - - - - - - - - - - - - - -//        ";

    /// <summary>
    /// Generates a strongly-typed class definition for application settings based on the provided JSON content.
    /// </summary>
    /// <param name="jsonContent">The JSON content representing the application settings.</param>
    /// <param name="namespaceName">The namespace to use for the generated class.</param>
    /// <param name="debugMsg">An optional debug message to include in the generated class.</param>
    /// <returns>A string containing the generated class definition.</returns>
    public static string GenerateDefinitionsClass(string jsonContent, string namespaceName, string debugMsg = null)
    {
        // Remove commented-out lines
        var filteredJsonContent = RemoveCommentsFromJson(jsonContent);
        string topLevelIndent = new(' ', 4);
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");  // First line
        sb.AppendLine();
        sb.AppendLine($"//{debugMsg}");
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("#pragma warning disable CA2211 // Non-constant fields should not be visible");
        sb.AppendLine("public class AppSettingsDefinitions");
        sb.AppendLine("{");

        var doc = JObject.Parse(filteredJsonContent);
        foreach (var element in doc)
        {
            if (element.Value is JObject)
                sb.AppendLine(GenerateClass(element, 1));
            else
                sb.AppendLine(GenerateClassPropertyPair(element, topLevelIndent));
        }

        sb.AppendLine("}");
        sb.AppendLine("#pragma warning restore CA2211 // Non-constant fields should not be visible");

        return sb.ToString();
    }

    //---------------------------------//

    private static string GenerateClass(KeyValuePair<string, JToken?> element, int indentLevel, bool isFromLogLevelClass = false)
    {
        var sb = new StringBuilder();
        string indent = new(' ', indentLevel * 4);
        string className = element.Key + "Section";

        sb.AppendLine($"{indent}");
        sb.AppendLine($"{indent}{_divider}");
        sb.AppendLine($"{indent}");

        sb.AppendLine($"{indent}public static class {className}");
        sb.AppendLine($"{indent}{{");
        sb.AppendLine($"{indent}    public const string Name = \"{element.Key}\";");

        foreach (var property in element.Value as JObject)
        {
            var isLogLevelClass = IsLogLevel(property);
            if (property.Value is JObject)
                sb.AppendLine(GenerateClass(property, indentLevel + 1, isLogLevelClass));
            else
                sb.AppendLine($"{GenerateClassPropertyPair(property, indent, isFromLogLevelClass)}");
        }

        sb.AppendLine($"{indent}}}");

        return sb.ToString();
    }

    //---------------------------------//

    private static string GenerateClassPropertyPair(KeyValuePair<string, JToken?> property, string indent, bool isFromLogLevelClass = false)
    {
        var sb = new StringBuilder();
        string keyName = SanitizeName(property.Key + "Key");
        string typeName = isFromLogLevelClass ? "LogLevel" : GetTypeName(property);
        string typePropertyName = SanitizeName(property.Key + "Type");

        sb.AppendLine($"{indent}");
        sb.AppendLine($"{indent}    public const string {keyName} = \"{property.Key}\";");
        sb.AppendLine($"{indent}    public static Type {typePropertyName} = typeof({typeName});");

        return sb.ToString();
    }

    //---------------------------------//

    private static string GetTypeName(KeyValuePair<string, JToken?> property)
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
            JTokenType.Array => GetArrayTypeName(element as JArray),
            _ => "object",
        };
    }

    //---------------------------------//

    private static string GetArrayTypeName(JArray arrayElement)
    {
        if (!arrayElement.Any())
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
        var lines = jsonContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var filteredLines = lines.Where(line => !line.TrimStart().StartsWith("//"));
        return string.Join("\r\n", filteredLines);
    }

    //---------------------------------//

    private static string SanitizeName(string name)
    {
        return name.Replace(".", "_");
    }

    //---------------------------------//

    internal static bool IsLogLevel(KeyValuePair<string, JToken?> property) =>
        property.Key.ToLowerInvariant().Contains("loglevel");

    //---------------------------------//

}//Cls


