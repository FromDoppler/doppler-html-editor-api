using System.Text.Json;

namespace Doppler.HtmlEditorApi.ApiModels;

public record Content(ContentType type, JsonElement? meta, string htmlContent);
