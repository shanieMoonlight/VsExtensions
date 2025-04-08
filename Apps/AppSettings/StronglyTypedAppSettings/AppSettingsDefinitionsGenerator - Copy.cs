//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace StronglyTypedAppSettings;


///// <summary>
///// Provides functionality to generate strongly-typed class definitions for application settings based on JSON content.
///// </summary>
//public class AppSettingsDefinitionsGenerator
//{
//    private const string _divider = "    //- - - - - - - - - - - - - - - -//        ";

//    /// <summary>
//    /// Generates a strongly-typed class definition for application settings based on the provided JSON content.
//    /// </summary>
//    /// <param name="jsonContent">The JSON content representing the application settings.</param>
//    /// <param name="namespaceName">The namespace to use for the generated class.</param>
//    /// <param name="debugMsg">An optional debug message to include in the generated class.</param>
//    /// <returns>A string containing the generated class definition.</returns>
//    public static string GenerateDefinitionsClass(string jsonContent, string namespaceName, string debugMsg = null)
//    {
//        // Remove commented-out lines
//        var filteredJsonContent = RemoveCommentsFromJson(jsonContent);
//        string topLevelIndent = new(' ', 4);
//        var sb = new StringBuilder();
//        sb.AppendLine("#nullable enable");  // First line
//        sb.AppendLine();
//        sb.AppendLine("using Microsoft.Extensions.Logging;");
//        sb.AppendLine();
//        sb.AppendLine();
//        sb.AppendLine($"//{debugMsg}");
//        sb.AppendLine($"namespace {namespaceName};");
//        sb.AppendLine();
//        sb.AppendLine();
//        sb.AppendLine("#pragma warning disable CA2211 // Non-constant fields should not be visible");
//        sb.AppendLine("public class AppSettingsDefinitions");
//        sb.AppendLine("{");

//        var doc = JObject.Parse(filteredJsonContent);
//        foreach (var element in doc)
//        {
//            if (element.Value is JObject)
//                sb.AppendLine(GenerateClass(element, 1));
//            else
//                sb.AppendLine(GenerateClassPropertyPair(element, topLevelIndent));
//        }

//        sb.AppendLine("}");
//        sb.AppendLine("#pragma warning restore CA2211 // Non-constant fields should not be visible");

//        return sb.ToString();
//    }

//    //---------------------------------//

//    /// <summary>
//    /// Generates a nested class definition for a JSON object within the application settings.
//    /// </summary>
//    /// <param name="element">The JSON element representing the object.</param>
//    /// <param name="indentLevel">The current indentation level for the generated class.</param>
//    /// <param name="isFromLogLevelClass">Indicates whether the class is part of a LogLevel section.</param>
//    /// <returns>A string containing the generated nested class definition.</returns>
//    private static string GenerateClass(KeyValuePair<string, JToken?> element, int indentLevel, bool isFromLogLevelClass = false)
//    {
//        var sb = new StringBuilder();
//        string indent = new(' ', indentLevel * 4);
//        string className = element.Key + "Section";

//        sb.AppendLine($"{indent}");
//        sb.AppendLine($"{indent}{_divider}");
//        sb.AppendLine($"{indent}");

//        sb.AppendLine($"{indent}public static class {className}");
//        sb.AppendLine($"{indent}{{");
//        sb.AppendLine($"{indent}    public const string Name = \"{element.Key}\";");

//        foreach (var property in element.Value as JObject)
//        {
//            var isLogLevelClass = IsLogLevel(property);
//            if (property.Value is JObject)
//                sb.AppendLine(GenerateClass(property, indentLevel + 1, isLogLevelClass));
//            else
//                sb.AppendLine($"{GenerateClassPropertyPair(property, indent, isFromLogLevelClass)}");
//        }

//        sb.AppendLine($"{indent}}}");

//        return sb.ToString();
//    }

//    //---------------------------------//

//    /// <summary>
//    /// Generates a property and type pair for a JSON property within the application settings.
//    /// </summary>
//    /// <param name="property">The JSON property to generate the pair for.</param>
//    /// <param name="indent">The current indentation level for the generated code.</param>
//    /// <param name="isFromLogLevelClass">Indicates whether the property is part of a LogLevel section.</param>
//    /// <returns>A string containing the generated property and type pair.</returns>
//    private static string GenerateClassPropertyPair(KeyValuePair<string, JToken?> property, string indent, bool isFromLogLevelClass = false)
//    {
//        var sb = new StringBuilder();
//        string keyName = SanitizeName(property.Key + "Key");
//        string typeName = isFromLogLevelClass ? "LogLevel" : GetTypeName(property);
//        string typePropertyName = SanitizeName(property.Key + "Type");

//        sb.AppendLine($"{indent}");
//        sb.AppendLine($"{indent}    public const string {keyName} = \"{property.Key}\";");
//        sb.AppendLine($"{indent}    public static Type {typePropertyName} = typeof({typeName});");

//        return sb.ToString();
//    }
//}


