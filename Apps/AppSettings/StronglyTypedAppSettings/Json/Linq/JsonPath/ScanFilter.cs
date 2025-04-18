using System.Collections.Generic;

namespace StronglyTyped.Newtonsoft.Json.Linq.JsonPath
{
    internal class ScanFilter : PathFilter
    {
        internal string? Name;

        public ScanFilter(string? name)
        {
            Name = name;
        }

        public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, JsonSelectSettings? settings)
        {
            foreach (JToken c in current)
            {
                if (Name == null)
                {
                    yield return c;
                }

                JToken? value = c;

                while (true)
                {
                    JContainer? container = value as JContainer;

                    value = GetNextScanValue(c, container, value);
                    if (value == null)
                    {
                        break;
                    }

                    if (value is JProperty property)
                    {
                        if (property.Name == Name)
                        {
                            yield return property.Value;
                        }
                    }
                    else
                    {
                        if (Name == null)
                        {
                            yield return value;
                        }
                    }
                }
            }
        }
    }
}