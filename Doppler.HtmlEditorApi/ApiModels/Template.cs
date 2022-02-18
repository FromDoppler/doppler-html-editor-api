using System.Text.Json;

namespace Doppler.HtmlEditorApi.ApiModels;

public record Template(ContentType type, string name, JsonElement? meta, string htmlContent) : Content(type, meta, htmlContent);
