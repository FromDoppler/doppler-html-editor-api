using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage;

public interface ICampaignContentRepository
{
    Task<CampaignState> GetCampaignState(string accountName, int campaignId);
    Task<ContentData> GetCampaignModel(string accountName, int campaignId);
    Task SaveCampaignContent(string accountName, ContentData contentRow);

    /// <summary>
    /// It keeps existing DB entries and only adds new ones without deleting anything.
    /// </summary>
    Task SaveNewFieldIds(int accountName, IEnumerable<int> fieldsId);

    /// <summary>
    /// It adds to DB the links that does not exist and removes the existing links that are not included in the payload.
    /// It does not update the properties of existing links that are also in the payload.
    /// </summary>
    Task SaveLinks(int ContentId, IEnumerable<string> links);
}
