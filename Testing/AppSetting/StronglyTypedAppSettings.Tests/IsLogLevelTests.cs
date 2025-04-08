using Shouldly;
using System.Collections.Generic;
using StronglyTyped.Newtonsoft.Json.Linq;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Tests for the IsLogLevel method
/// </summary>
public class IsLogLevelTests
{
    [Fact]
    public void IsLogLevel_WithExactLogLevelName_ReturnsTrue()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("LogLevel", JTokenType.String, "Information");
        
        // Act
        bool result = InvokePrivateStaticMethod<bool>(
            typeof(AppSettingsDefinitionsGenerator), 
            "IsLogLevel", 
            [property]);
        
        // Assert
        result.ShouldBeTrue();
    }

    //-------------------------------------//

    [Fact]
    public void IsLogLevel_WithLogLevelInName_ReturnsTrue()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("DefaultLogLevel", JTokenType.String, "Information");
        
        // Act
        bool result = InvokePrivateStaticMethod<bool>(
            typeof(AppSettingsDefinitionsGenerator), 
            "IsLogLevel", 
            [property]);
        
        // Assert
        result.ShouldBeTrue();
    }

    //-------------------------------------//

    [Fact]
    public void IsLogLevel_WithMixedCaseLogLevel_ReturnsTrue()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("defaultloglevel", JTokenType.String, "Information");
        
        // Act
        bool result = InvokePrivateStaticMethod<bool>(
            typeof(AppSettingsDefinitionsGenerator), 
            "IsLogLevel", 
            [property]);
        
        // Assert
        result.ShouldBeTrue();
    }

    //-------------------------------------//

    [Fact]
    public void IsLogLevel_WithoutLogLevelInName_ReturnsFalse()
    {
        // Arrange
        var property = CreatePropertyKeyValuePair("OtherSetting", JTokenType.String, "Information");
        
        // Act
        bool result = InvokePrivateStaticMethod<bool>(
            typeof(AppSettingsDefinitionsGenerator), 
            "IsLogLevel", 
            [property]);
        
        // Assert
        result.ShouldBeFalse();
    }

    //-------------------------------------//

    private static KeyValuePair<string, JToken?> CreatePropertyKeyValuePair(string key, JTokenType tokenType, object value)
    {
        JToken token = new JValue(value);
        return new KeyValuePair<string, JToken?>(key, token);
    }

    //-------------------------------------//

    private static T InvokePrivateStaticMethod<T>(Type type, string methodName, object[] parameters)
    {
        MethodInfo method = type.GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception($"Method {methodName} not found in {type.Name}");
        return (T)method.Invoke(null, parameters);
    }

}//Cls
