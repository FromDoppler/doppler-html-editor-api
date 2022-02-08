using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model;

public record Content(ContentType type, JsonElement? meta, string htmlContent);
