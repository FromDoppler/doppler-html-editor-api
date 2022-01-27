using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model;

public record Content(JsonElement meta, string htmlContent);
