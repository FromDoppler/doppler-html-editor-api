using System.Text.Json;

namespace Doppler.HtmlEditorApi.ApiModels;

public abstract record Content(ContentType type, JsonElement? meta, string htmlContent);
