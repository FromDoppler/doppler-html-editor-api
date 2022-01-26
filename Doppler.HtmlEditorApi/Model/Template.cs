using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model;

public record Template(string name, JsonElement meta, string htmlContent) : Content(meta, htmlContent);
