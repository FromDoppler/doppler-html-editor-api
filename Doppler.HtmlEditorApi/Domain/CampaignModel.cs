namespace Doppler.HtmlEditorApi.Domain;

public record CampaignModel(
    int CampaignId,
    string Name,
    string PreviewImage,
    CampaignContentData Content);
