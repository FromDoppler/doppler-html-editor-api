using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.ApiModels;

public record Template(
    string templateName,
    bool isPublic,
    string previewImage,
    string htmlContent,
    JsonElement? meta) : Content(ContentType.unlayer, meta, htmlContent);
