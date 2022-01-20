using System.Text.Json;
using Doppler.HtmlEditorApi.Model;

public class CampaignContentRequest
{
    public string Content { get; set; }
    public JsonElement Meta { get; set; }
}
