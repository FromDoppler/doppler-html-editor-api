using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model;

public record CampaignContent(JsonElement meta, string htmlContent) : Content(meta, htmlContent);
