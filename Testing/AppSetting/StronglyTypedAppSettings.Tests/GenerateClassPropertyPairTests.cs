using Shouldly;
using StronglyTyped.Newtonsoft.Json.Linq;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the GenerateClassPropertyPair method
/// </summary>
public class GenerateClassPropertyPairTests
{
    [Fact]
    public void GenerateClassPropertyPair_WithRegularProperty_GeneratesPropertyDeclarations()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("TestProperty", JTokenType.String, "value");
        string indent = "    ";
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GenerateClassPropertyPair", 
            [property, indent, false]);
        
        // Assert
        result.ShouldContain("public const string TestPropertyKey = \"TestProperty\";");
        result.ShouldContain("public static Type TestPropertyType = typeof(string);");
    }

    [Fact]
    public void GenerateClassPropertyPair_WithLogLevelProperty_UsesLogLevelType()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("Default", JTokenType.String, "Information");
        string indent = "    ";
        bool isFromLogLevelClass = true;
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GenerateClassPropertyPair", 
            [property, indent, isFromLogLevelClass]);
        
        // Assert
        result.ShouldContain("public const string DefaultKey = \"Default\";");
        result.ShouldContain("public static Type DefaultType = typeof(LogLevel);");
    }
    
    private static KeyValuePair<string, JToken?> CreatePropertyKeyValuePair(string key, JTokenType tokenType, object value)
    {
        JToken token = tokenType switch
        {
            JTokenType.String => new JValue((string)value),
            JTokenType.Integer => new JValue((int)value),
            JTokenType.Float => new JValue((double)value),
            JTokenType.Boolean => new JValue((bool)value),
            JTokenType.Null => JValue.CreateNull(),
            _ => new JValue(value?.ToString()),
        };
        return new KeyValuePair<string, JToken?>(key, token);
    }
    
    private static T InvokePrivateStaticMethod<T>(Type type, string methodName, object[] parameters)
    {
        MethodInfo method = type.GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception($"Method {methodName} not found in {type.Name}");
        return (T)method.Invoke(null, parameters);
    }
}
