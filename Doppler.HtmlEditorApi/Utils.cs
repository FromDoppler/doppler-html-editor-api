using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Doppler.HtmlEditorApi;

public static class Utils
{
    public static JsonElement ParseAsJsonElement(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        => source ?? Enumerable.Empty<T>();

    public static string FallbackIfNullOrEmpty(this string source, string fallback)
        => string.IsNullOrEmpty(source) ? fallback : source;
}
