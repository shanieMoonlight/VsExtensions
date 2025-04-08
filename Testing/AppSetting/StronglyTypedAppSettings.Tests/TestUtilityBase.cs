using StronglyTyped.Newtonsoft.Json.Linq;
using System.Reflection;

namespace StronglyTypedAppSettings.Tests;

/// <summary>
/// Base test utility class to provide common private method invocation 
/// </summary>
public abstract class TestUtilityBase
{
    protected static T InvokePrivateStaticMethod<T>(Type type, string methodName, object[] parameters)
    {
        MethodInfo method = type.GetMethod(methodName, 
            BindingFlags.NonPublic | BindingFlags.Static) ?? throw new Exception($"Method {methodName} not found in {type.Name}");
        return (T)method.Invoke(null, parameters);
    }

    //-------------------------------------//

    protected static KeyValuePair<string, JToken?> CreatePropertyKeyValuePair(string key, JTokenType tokenType, object value)
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

}//Cls
