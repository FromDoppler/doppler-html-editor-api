using System.Text.Json;

namespace Doppler.HtmlEditorApi;

public static class Utils
{
    public static JsonElement ParseAsJsonElement(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
