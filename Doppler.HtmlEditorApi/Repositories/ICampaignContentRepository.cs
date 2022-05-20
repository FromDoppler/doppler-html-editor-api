using System.Collections.Generic;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.Domain;

namespace Doppler.HtmlEditorApi.Repositories;

public interface ICampaignContentRepository
{
    Task<CampaignState> GetCampaignState(string accountName, int campaignId);
    Task<ContentData> GetCampaignModel(string accountName, int campaignId);
    Task CreateCampaignContent(ContentData content);
    Task UpdateCampaignContent(ContentData content);

    /// <summary>
    /// It keeps existing DB entries and only adds new ones without deleting anything.
    /// </summary>
    Task SaveNewFieldIds(int contentId, IEnumerable<int> fieldsId);

    /// <summary>
    /// It adds to DB the links that does not exist and removes the existing links that are not included in the payload.
    /// It does not update the properties of existing links that are also in the payload.
    /// </summary>
    Task SaveLinks(int contentId, IEnumerable<string> links);
    Task UpdateCampaignStatus(int setCurrentStep, int setHtmlSourceType, int whenIdCampaignIs, int whenCurrentStepIs);
    Task UpdateCampaignPreviewImage(int campaignId, string previewImage);
}
