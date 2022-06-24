using System.Text.Json;

namespace Doppler.HtmlEditorApi.ApiModels;

public record Template(string htmlCode, JsonElement? meta, string previewImage);
