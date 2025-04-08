using Shouldly;
using StronglyTyped.Newtonsoft.Json.Linq;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the GetArrayTypeName method
/// </summary>
public class GetArrayTypeNameTests
{
    [Fact]
    public void GetArrayTypeName_WithEmptyArray_ReturnsStringArrayType()
    {
        // Arrange
        var array = new JArray();
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetArrayTypeName", 
            [array]);
        
        // Assert
        result.ShouldBe("string[]");
    }

    //-------------------------------------//

    [Fact]
    public void GetArrayTypeName_WithStringElements_ReturnsStringArrayType()
    {
        // Arrange
        var array = new JArray("value1", "value2");
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetArrayTypeName", 
            [array]);
        
        // Assert
        result.ShouldBe("string[]");
    }

    //-------------------------------------//

    [Fact]
    public void GetArrayTypeName_WithIntegerElements_ReturnsIntArrayType()
    {
        // Arrange
        var array = new JArray(1, 2, 3);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetArrayTypeName", 
            [array]);
        
        // Assert
        result.ShouldBe("int[]");
    }

    //-------------------------------------//

    [Fact]
    public void GetArrayTypeName_WithFloatElements_ReturnsDoubleArrayType()
    {
        // Arrange
        var array = new JArray(1.1, 2.2, 3.3);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetArrayTypeName", 
            [array]);
        
        // Assert
        result.ShouldBe("double[]");
    }

    //-------------------------------------//

    [Fact]
    public void GetArrayTypeName_WithBooleanElements_ReturnsBoolArrayType()
    {
        // Arrange
        var array = new JArray(true, false);
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetArrayTypeName", 
            [array]);
        
        // Assert
        result.ShouldBe("bool[]");
    }

    //-------------------------------------//

    [Fact]
    public void GetArrayTypeName_WithComplexElements_ReturnsStringArrayType()
    {
        // Arrange
        var array = new JArray(new JObject(), new JObject());
        
        // Act
        string result = InvokePrivateStaticMethod<string>(
            typeof(AppSettingsDefinitionsGenerator), 
            "GetArrayTypeName", 
            [array]);
        
        // Assert
        result.ShouldBe("string[]");
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
