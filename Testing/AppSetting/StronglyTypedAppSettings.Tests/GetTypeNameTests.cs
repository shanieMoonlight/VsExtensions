using Shouldly;
using StronglyTyped.Newtonsoft.Json.Linq;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the GetTypeName method
/// </summary>
public class GetTypeNameTests
{
    [Fact]
    public void GetTypeName_WithStringValue_ReturnsStringType()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("testKey", JTokenType.String, "testValue");
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetTypeName", 
            [property]);
        
        // Assert
        result.ShouldBe("string");
    }

    //-------------------------------------//

    [Fact]
    public void GetTypeName_WithIntegerValue_ReturnsIntType()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("testKey", JTokenType.Integer, 42);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetTypeName", 
            [property]);
        
        // Assert
        result.ShouldBe("int");
    }

    //-------------------------------------//

    [Fact]
    public void GetTypeName_WithFloatValue_ReturnsDoubleType()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("testKey", JTokenType.Float, 42.5);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetTypeName", 
            [property]);
        
        // Assert
        result.ShouldBe("double");
    }

    //-------------------------------------//

    [Fact]
    public void GetTypeName_WithBooleanValue_ReturnsBoolType()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("testKey", JTokenType.Boolean, true);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetTypeName", 
            [property]);
        
        // Assert
        result.ShouldBe("bool");
    }

    //-------------------------------------//

    [Fact]
    public void GetTypeName_WithArrayValue_CallsGetArrayTypeNameMethod()
    {
        // Arrange
        var array = new JArray("value1", "value2");
        var property = CreatePropertyKeyValuePair("testKey", JTokenType.Array, array);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetTypeName", 
            [property]);
        
        // Assert
        result.ShouldBe("string[]");
    }

    //-------------------------------------//

    [Fact]
    public void GetTypeName_WithOtherType_ReturnsObjectType()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("testKey", JTokenType.Null, null);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetTypeName", 
            [property]);
        
        // Assert
        result.ShouldBe("object");
    }

    [Fact]
    public void GetTypeName_WithLogLevelInName_ReturnsLogLevelType()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("LogLevel", JTokenType.String, "Information");
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetTypeName", 
            [property]);
        
        // Assert
        result.ShouldBe("LogLevel");
    }

    //-------------------------------------//

    private static KeyValuePair<string, JToken?> CreatePropertyKeyValuePair(string key, JTokenType tokenType, object value)
    {
        JToken token = tokenType switch
        {
            JTokenType.String => new JValue((string)value),
            JTokenType.Integer => new JValue((int)value),
            JTokenType.Float => new JValue((double)value),
            JTokenType.Boolean => new JValue((bool)value),
            JTokenType.Array => (JArray)value,
            JTokenType.Null => JValue.CreateNull(),
            _ => new JValue(value?.ToString()),
        };
        return new KeyValuePair<string, JToken?>(key, token);
    }

    //-------------------------------------//

    private static T InvokePrivateStaticMethod<T>(Type type, string methodName, object[] parameters)
    {
        MethodInfo method = type.GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception($"Method {methodName} not found in {type.Name}");
        return (T)method.Invoke(null, parameters);
    }

    //-------------------------------------//
}
