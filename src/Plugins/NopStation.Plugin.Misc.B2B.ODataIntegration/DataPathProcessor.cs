using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NopStation.Plugin.Misc.B2B.ODataIntegration.Exceptions;

namespace NopStation.Plugin.Misc.B2B.ODataIntegration;

public class DataPathProcessor
{
    private readonly JObject? _jObject;
    private readonly XDocument? _xDocument;
    private readonly DataFormat _format;

    private enum DataFormat
    {
        Json = 1,
        Xml,
    }

    public DataPathProcessor(JObject jObject)
    {
        _jObject = jObject ?? throw new ArgumentNullException(nameof(jObject));
        _format = DataFormat.Json;
        _xDocument = null;
    }

    public DataPathProcessor(XDocument xDocument)
    {
        _xDocument = xDocument ?? throw new ArgumentNullException(nameof(xDocument));
        _format = DataFormat.Xml;
        _jObject = null;
    }

    public DataPathProcessor(string content)
    {
        if (string.IsNullOrEmpty(content))
            throw new ArgumentException("Content cannot be null or empty", nameof(content));

        (_format, _jObject, _xDocument) = DetectFormat(content);
    }

    public List<T?> GetValues<T>(string pathExpression)
    {
        if (string.IsNullOrEmpty(pathExpression))
        {
            throw new ArgumentException(
                "Path expression cannot be null or empty",
                nameof(pathExpression)
            );
        }

        if (!pathExpression.StartsWith('$') && !pathExpression.StartsWith('/'))
        {
            try
            {
                var value = (T?)Convert.ChangeType(pathExpression, typeof(T));

                return [value];
            }
            catch (Exception ex)
                when (ex is InvalidCastException or FormatException or OverflowException)
            {
                throw new DataPathEvaluationException(
                    $"Failed to convert literal value to type {typeof(T).Name}",
                    pathExpression,
                    ex
                );
            }
        }

        try
        {
            var rawValues = _format switch
            {
                DataFormat.Json => GetJsonValues<object>(pathExpression),
                DataFormat.Xml => GetXmlValues<object>(pathExpression),
                _ => throw new NotSupportedException("Unknown data format"),
            };

            return rawValues
                .Select(rawValue =>
                {
                    try
                    {
                        if (rawValue is JToken token)
                            return token.ToObject<T>();

                        if (rawValue is T value)
                            return value;

                        return (T?)Convert.ChangeType(rawValue, typeof(T));
                    }
                    catch (Exception ex)
                        when (ex is InvalidCastException or FormatException or OverflowException)
                    {
                        throw new DataPathEvaluationException(
                            $"Failed to convert value '{rawValue}' to type {typeof(T).Name}",
                            pathExpression,
                            ex
                        );
                    }
                })
                .ToList();
        }
        catch (Exception ex) when (ex is JsonException or XmlException)
        {
            throw new DataPathEvaluationException("Failed to evaluate path", pathExpression, ex);
        }
    }

    public T MapToObject<T>(Dictionary<string, string?> propertyMappings, string propertyName = "",
        Func<string, object?, object?>? customConverter = null) where T : new()
    {
        var propertyInfoCache = new Dictionary<string, PropertyInfo>();
        var propertyTypeCache = new Dictionary<string, Type>();
        var type = typeof(T);

        var valueMap = new Dictionary<string, List<object?>>();

        foreach (var (keyName, pathExpression) in propertyMappings)
        {
            if (pathExpression == null)
                continue;

            if (string.IsNullOrWhiteSpace(propertyName))
                propertyName = keyName;

            var values = GetValues<object>(pathExpression);
            valueMap[propertyName] = values;
        }

        foreach (var keyName in valueMap.Keys)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                propertyName = keyName;

            var propertyInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propertyInfo == null || !propertyInfo.CanWrite)
                continue;

            propertyInfoCache[propertyName] = propertyInfo;
            propertyTypeCache[propertyName] =
                Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        }

        var obj = new T();  // Single instance

        foreach (var (keyName, values) in valueMap)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                propertyName = keyName;

            if (!propertyInfoCache.TryGetValue(propertyName, out var propertyInfo))
                continue;

            var rawValue = values.Count switch
            {
                1 => values[0],
                _ => null
            };

            try
            {
                var convertedValue = rawValue;

                if (customConverter != null)
                {
                    convertedValue = customConverter(propertyName, rawValue);
                }
                else if (rawValue != null)
                {
                    var targetType = propertyTypeCache[propertyName];
                    convertedValue = Convert.ChangeType(rawValue, targetType);
                }

                propertyInfo.SetValue(obj, convertedValue);
            }
            catch (Exception ex)
            {
                throw new DataPathEvaluationException(
                    $"Failed to map value for property '{propertyName}'. " +
                    $"Value: '{rawValue}', Target Type: {propertyInfo.PropertyType.Name}",
                    propertyMappings[propertyName]!,
                    ex);
            }
        }

        return obj;  // Return a single instance
    }

    public List<T> MapToObjects<T>(Dictionary<string, string?> propertyMappings)
            where T : new()
    {
        var propertyChainCache = new Dictionary<string, List<PropertyInfo>>();
        var targetTypeCache = new Dictionary<string, Type>();
        var type = typeof(T);

        var valueMap = new Dictionary<string, List<object?>>();
        var maxItems = 0;
        foreach (var (propertyName, pathExpr) in propertyMappings)
        {
            if (pathExpr == null)
                continue;
            var vals = GetValues<object>(pathExpr);
            valueMap[propertyName] = vals;

            if (pathExpr.StartsWith('$') || pathExpr.StartsWith('/'))
            {
                maxItems = Math.Max(maxItems, vals.Count);
            }
        }


        var literalKeys = propertyMappings
            .Where(kvp =>
                kvp.Value != null
                && !kvp.Value!.StartsWith('$')
                && !kvp.Value!.StartsWith('/'))
            .Select(kvp => kvp.Key);

        foreach (var key in literalKeys)
        {
            if (valueMap.TryGetValue(key, out var list) && list.Count > 0)
            {
                var literal = list[0];
                valueMap[key] = Enumerable
                    .Repeat(literal, maxItems)
                    .ToList();
            }
        }

        foreach (var propertyName in propertyMappings.Keys)
        {
            if (!valueMap.ContainsKey(propertyName))
                continue;
            var segments = propertyName.Split('.');
            var chain = new List<PropertyInfo>();
            var currType = type;
            var ok = true;
            foreach (var seg in segments)
            {
                var pi = currType.GetProperty(seg, BindingFlags.Instance | BindingFlags.Public);
                if (pi == null || !pi.CanWrite)
                {
                    ok = false;
                    break;
                }

                chain.Add(pi);
                currType = pi.PropertyType;
            }

            if (!ok)
                continue;
            propertyChainCache[propertyName] = chain;
            var lastProperty = chain?.LastOrDefault();
            var lastType = lastProperty == null
                ? null
                : Nullable.GetUnderlyingType(lastProperty.PropertyType) ?? lastProperty.PropertyType;

            targetTypeCache[propertyName] = lastType;
        }

        var rootPaths = propertyChainCache.Keys.Select(k => k.Split('.')[0]).Distinct().ToList();
        var rootValueMap = new Dictionary<string, List<object?>>();
        foreach (var root in rootPaths)
        {
            string expr;
            if (propertyMappings.TryGetValue(root, out var directExpr) && directExpr != null)
            {
                expr = directExpr;
            }
            else
            {
                var nestedExpr = propertyMappings
                    .First(kvp => kvp.Key.StartsWith(root + "."))
                    .Value!;
                var dotIdx = nestedExpr.LastIndexOf('.');
                expr = dotIdx > 0 ? nestedExpr[..dotIdx] : nestedExpr;
            }

            var list = GetValues<object>(expr);

            if (!expr.StartsWith('$') && !expr.StartsWith('/'))
            {
                var literal = list.FirstOrDefault();
                list = Enumerable.Repeat(literal, maxItems).ToList();
            }
            else if (list.Count < maxItems)
            {
                for (var pad = list.Count; pad < maxItems; pad++)
                    list.Add(null);
            }

            rootValueMap[root] = list;

        }

        var result = new List<T>(new T[maxItems]);
        Parallel.For(
            0,
            maxItems,
            i =>
            {
                var obj = new T();

                foreach (var root in rootPaths)
                {
                    var rawRoot = rootValueMap[root][i];
                    var rootProp = type.GetProperty(
                        root,
                        BindingFlags.Instance | BindingFlags.Public
                    );
                    if (rawRoot == null && rootProp != null)
                    {
                        rootProp.SetValue(obj, null);
                    }
                }

                foreach (var (propName, vals) in valueMap)
                {
                    if (!propertyChainCache.TryGetValue(propName, out var chain))
                        continue;

                    var root = chain[0].Name;
                    if (rootValueMap[root][i] == null)
                        continue;

                    var raw = i < vals.Count ? vals[i] : null;
                    var targetType = targetTypeCache[propName];

                    object? converted = null;
                    if (raw != null)
                    {
                        if (IsComplexType(targetType))
                        {
                            converted = JsonConvert.DeserializeObject(
                                JsonConvert.SerializeObject(raw),
                                targetType
                            );
                        }
                        else
                        {
                            try
                            {
                                converted = Convert.ChangeType(raw, targetType);
                            }
                            catch
                            {
                                converted = null;
                            }
                        }
                    }

                    object? current = obj;
                    for (var depth = 0; depth < chain.Count - 1; depth++)
                    {
                        var pi = chain[depth];
                        var child = pi.GetValue(current);
                        if (child == null)
                        {
                            child = Activator.CreateInstance(pi.PropertyType)!;
                            pi.SetValue(current, child);
                        }

                        current = child;
                    }

                    var lastProperty = chain?.LastOrDefault();

                    if (lastProperty != null && current != null)
                    {
                        lastProperty.SetValue(current, converted);
                    }

                }

                result[i] = obj;
            }
        );

        return result;
    }

    private List<T?> GetJsonValues<T>(string pathExpression)
    {
        if (!pathExpression.StartsWith('$'))
        {
            throw new ArgumentException("JSON path expressions must start with '$'", nameof(pathExpression));
        }

        return _jObject!
            .SelectTokens(pathExpression)
            .Select(token => token.ToObject<T>())
            .ToList();
    }

    private List<T?> GetXmlValues<T>(string pathExpression)
    {
        if (!pathExpression.StartsWith('/'))
        {
            throw new ArgumentException("XML path expressions should start with '/'", nameof(pathExpression));
        }

        var elements = _xDocument!.XPathSelectElements(pathExpression);

        return elements.Select(e =>
        {
            if (typeof(T) == typeof(string))
            {
                return (T?)(object)e.Value;
            }

            return (T?)Convert.ChangeType(e.Value, typeof(T));
        }).ToList();
    }

    private static (DataFormat format, JObject? jObject, XDocument? xDocument) DetectFormat(string content)
    {
        try
        {
            var jObject = JObject.Parse(content);
            return (DataFormat.Json, jObject, null);
        }
        catch (JsonReaderException)
        {
            try
            {
                var xDocument = XDocument.Parse(content);
                return (DataFormat.Xml, null, xDocument);
            }
            catch (XmlException ex)
            {
                throw new ArgumentException("Content is neither valid JSON nor XML", nameof(content), ex);
            }
        }
    }

    private static bool IsComplexType(Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsClass || type is { IsValueType: true, IsPrimitive: false, IsEnum: false };
    }
}

    