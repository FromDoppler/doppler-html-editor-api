using System.Text.Json;

namespace Doppler.HtmlEditorApi.ApiModels;

public record Template(
    string name,
    bool isPublic,
    string previewImage,
    string htmlContent,
    JsonElement? meta)
{
    public string type { get; } = "unlayer";
}
