using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage;

public interface ICampaignContentRepository
{
    Task<ContentData> GetCampaignModel(string accountName, int campaignId);
    Task SaveCampaignContent(string accountName, ContentData contentRow);

    /// <summary>
    /// It keeps existing DB entries and only adds new ones without deleting anything.
    /// </summary>
    Task SaveNewFieldIds(int accountName, IEnumerable<int> fieldsId);
}
