using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model;

public record CampaignContent(
    [Required]
    JsonElement meta,
    [Required]
    string htmlContent) : Content(meta, htmlContent);
