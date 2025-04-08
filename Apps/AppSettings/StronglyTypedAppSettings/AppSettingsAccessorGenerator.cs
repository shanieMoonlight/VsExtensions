using StronglyTypedAppSettings.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StronglyTypedAppSettings;


/// <summary>
/// Generates accessor classes for strongly-typed application settings.
/// This class parses application settings definition classes and creates corresponding accessor classes
/// that enable type-safe access to configuration values.
/// </summary>
public class AppSettingsAccessorGenerator
{
    private static readonly string _separator = "//---------------------------------//";
    private static readonly string _tab = "\t";

    /// <summary>
    /// Generates a strongly-typed accessor class for application settings based on the provided definition class.
    /// </summary>
    /// <param name="appSettingsDefinitionsClassAsString">The string representation of the application settings definition class.</param>
    /// <param name="namespaceName">The namespace to use for the generated accessor class.</param>
    /// <param name="debugMsg">Optional debug message to include in the generated code.</param>
    /// <returns>A string containing the complete C# code for the accessor class.</returns>
    public static string GenerateAccessorClass(string appSettingsDefinitionsClassAsString, string namespaceName, string debugMsg = null)
    {
        var sb = HandleBeginClassFileAndReturnBuilder(namespaceName, debugMsg);

        var classStack = new Stack<string>();
        var lines = appSettingsDefinitionsClassAsString.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            var trimmedLine = lines[i].Trim();

            if (trimmedLine.StartsWith("public static class"))
                HandleClassStart(classStack, sb, lines, i);
            else if (trimmedLine.StartsWith("public const string Name"))
                HandleSectionName(classStack, sb);
            else if (trimmedLine.StartsWith("public const string"))
                i = HandleTypeValuePair(classStack, sb, lines, i);
            else if (trimmedLine == "}")
                HandleClassEnding(classStack, sb);
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    //-------------------------------//

    /// <summary>
    /// Creates the beginning of the accessor class file including namespaces, imports, and class declaration.
    /// </summary>
    /// <param name="namespaceName">The namespace name to use in the generated code.</param>
    /// <param name="debugMsg">Optional debug message to include as a comment.</param>
    /// <returns>A StringBuilder with the initial part of the file already appended.</returns>
    private static StringBuilder HandleBeginClassFileAndReturnBuilder(string namespaceName, string debugMsg = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");  // First line
        sb.AppendLine();
        sb.AppendLine($"//{debugMsg}");
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.Configuration;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("public class AppSettingsAccessor(IConfiguration _config)");
        sb.AppendLine("{");
        sb.AppendLine("");
        sb.AppendLine($"{_tab}private readonly IConfiguration Config = _config;");
        sb.AppendLine("");
        sb.AppendLine($"{_tab}//---------------------------------//");
        sb.AppendLine("");

        return sb;
    }

    //-------------------------------//

    /// <summary>
    /// Handles the start of a new class definition in the application settings.
    /// Creates a corresponding accessor class in the generated code.
    /// </summary>
    /// <param name="classStack">Stack tracking nested class hierarchy.</param>
    /// <param name="sb">StringBuilder to append generated code to.</param>
    /// <param name="lines">All lines from the original definition file.</param>
    /// <param name="currentIdx">Current line index being processed.</param>
    private static void HandleClassStart(Stack<string> classStack, StringBuilder sb, string[] lines, int currentIdx)
    {
        var trimmedLine = lines[currentIdx].Trim();
        var className = trimmedLine.Split(' ')[3];
        classStack.Push(className);
        sb.AppendLine();
        sb.AppendLine($"{_tab}{_separator}");
        sb.AppendLine();
        sb.AppendLine($"{_tab}public {className}Accessor {className} = new(_config);");
        sb.AppendLine($"{_tab}public class {className}Accessor(IConfiguration _config)");
        sb.AppendLine($"{_tab}{{");
    }

    //-------------------------------//  

    /// <summary>
    /// Handles the end of a class definition by appending closing brackets and comments.
    /// </summary>
    /// <param name="classStack">Stack tracking nested class hierarchy.</param>
    /// <param name="sb">StringBuilder to append generated code to.</param>
    private static void HandleClassEnding(Stack<string> classStack, StringBuilder sb)
    {
        if (classStack.Count <= 0)
            return;

        var className = classStack.Pop();
        sb.AppendLine($"{_tab}}}//Cls - {className}Section");
        sb.AppendLine();
    }

    //-------------------------------//  

    /// <summary>
    /// Handles section name definitions by creating the corresponding configuration section access code.
    /// </summary>
    /// <param name="classStack">Stack tracking nested class hierarchy.</param>
    /// <param name="sb">StringBuilder to append generated code to.</param>
    private static void HandleSectionName(Stack<string> classStack, StringBuilder sb)
    {
        var sectionNameParts = classStack.Reverse()
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var sectionName = string.Join(".", sectionNameParts);

        sb.AppendLine($"{_tab}{_tab}public readonly IConfigurationSection Config = _config.GetSection(AppSettingsDefinitions.{sectionName}.Name);");
        sb.AppendLine();
    }

    //-------------------------------//       

    /// <summary>
    /// Handles type-value pairs defined in the application settings by generating getter methods.
    /// </summary>
    /// <param name="classStack">Stack tracking nested class hierarchy.</param>
    /// <param name="sb">StringBuilder to append generated code to.</param>
    /// <param name="lines">All lines from the original definition file.</param>
    /// <param name="currentIdx">Current line index being processed.</param>
    /// <returns>The updated line index after processing the type-value pair.</returns>
    private static int HandleTypeValuePair(Stack<string> classStack, StringBuilder sb, string[] lines, int currentIdx)
    {
        var trimmedLine = lines[currentIdx].Trim();
        var keyName = trimmedLine.Split(' ')[3];
        string typeLine = "";
        while (string.IsNullOrWhiteSpace(typeLine) && ++currentIdx < lines.Length)
        {
            typeLine = lines[currentIdx].Trim();
        }

        if (string.IsNullOrWhiteSpace(typeLine))
            return currentIdx;

        if (typeLine.StartsWith("public static Type") && typeLine.Contains(keyName.ReplaceLastOccurrence("Key", "Type")))
        {
            var returnType = GetTypeValue(typeLine);
            var methodName = keyName.ReplaceLastOccurrence("Key", string.Empty);
            sb.AppendLine($"{_tab}{_tab}public {returnType} Get{methodName}() =>");


            var sectionNameParts = new List<string> { "AppSettingsDefinitions", string.Join(".", classStack.Reverse()), keyName }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            var sectionName = string.Join(".", sectionNameParts);


            sb.AppendLine($"{_tab}{_tab}{_tab}Config.GetSection({sectionName})");
            sb.AppendLine($"{_tab}{_tab}{_tab}.Get<{returnType}?>() ?? {GetDefaultValue(returnType)};");
            sb.AppendLine();
        }

        return currentIdx;
    }

    //-------------------------------//       

    /// <summary>
    /// Extracts the type value from a line containing typeof(...).
    /// </summary>
    /// <param name="line">The line containing the type definition.</param>
    /// <returns>The extracted type name as a string.</returns>
    public static string GetTypeValue(string line)
    {
        string pattern = @"typeof\((.*?)\)";
        Match match = Regex.Match(line, pattern);

        return match.Success
            ? match.Groups[1].Value
            : "string";
    }

    //- - - - - - - - - - - - - - - -//       

    /// <summary>
    /// Returns the default value for a given type as a string.
    /// </summary>
    /// <param name="propType">The property type to get a default value for.</param>
    /// <returns>A string representation of the default value for the specified type.</returns>
    internal static string GetDefaultValue(string propType)
    {
        var propTypeLower = propType.Trim().ToLower();

        if (propTypeLower.EndsWith("loglevel"))
            return "LogLevel.None";

        if (propTypeLower.EndsWith("[]"))
            return "[]";

        return propTypeLower switch
        {
            "string" => "string.Empty",
            "int" => "0",
            "double" => "0",
            "bool" => "false",
            "datetime" => "default",
            "byte" => "default",
            "guid" => "Guid.Empty",
            _ => "default",
        };
    }

    //-------------------------------//       

}//Cls
