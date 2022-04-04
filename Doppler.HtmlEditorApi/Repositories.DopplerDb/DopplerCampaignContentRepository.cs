using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb;

public class DapperCampaignContentRepository : ICampaignContentRepository
{
    private const int EDITOR_TYPE_MSEDITOR = 4;
    private const int EDITOR_TYPE_UNLAYER = 5;
    private const int DOPPLER_CAMPAIGN_STATUS_DRAFT = 1;
    private const int DOPPLER_CAMPAIGN_STATUS_AB_DRAFT = 11;
    private const int DOPPLER_CAMPAIGN_STATUS_IN_WINNER_IN_AB_SELECTION_PROCESS = 18;

    private readonly IDbContext _dbContext;
    public DapperCampaignContentRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ContentData> GetCampaignModel(string accountName, int campaignId)
    {
        var queryResult = await _dbContext.ExecuteAsync(new FirstOrDefaultContentWithCampaignStatusDbQuery(
            IdCampaign: campaignId,
            AccountName: accountName
        ));

        if (queryResult == null || !queryResult.CampaignExists)
        {
            return null;
        }

        if (!queryResult.CampaignHasContent)
        {
            return new EmptyContentData(campaignId);
        };

        if (queryResult.EditorType == EDITOR_TYPE_MSEDITOR)
        {
            return new MSEditorContentData(campaignId, queryResult.Content);
        }

        if (queryResult.EditorType == EDITOR_TYPE_UNLAYER)
        {
            return new UnlayerContentData(
                campaignId: queryResult.IdCampaign,
                htmlContent: queryResult.Content,
                htmlHead: queryResult.Head,
                meta: queryResult.Meta);
        }

        if (queryResult.EditorType == null)
        {
            return new HtmlContentData(
                campaignId: queryResult.IdCampaign,
                htmlContent: queryResult.Content,
                htmlHead: queryResult.Head);
        }

        return new UnknownContentData(
            campaignId: queryResult.IdCampaign,
            content: queryResult.Content,
            head: queryResult.Head,
            meta: queryResult.Meta,
            editorType: queryResult.EditorType);
    }

    public async Task<CampaignState> GetCampaignState(string accountName, int campaignId)
    {
        var campaignStateData = await _dbContext.ExecuteAsync(new FirstOrDefaultCampaignStatusDbQuery(
            AccountName: accountName,
            IdCampaign: campaignId
        ));

        if (campaignStateData == null || !campaignStateData.OwnCampaignExists)
        {
            return new NoExistCampaignState();
        }

        // For information about Doppler's status code, check out here
        // https://github.com/MakingSense/Doppler/blob/develop/Doppler.Transversal/Classes/CampaignStatusEnum.cs
        var campaignStatus = campaignStateData.Status == DOPPLER_CAMPAIGN_STATUS_DRAFT ||
            campaignStateData.Status == DOPPLER_CAMPAIGN_STATUS_AB_DRAFT ? CampaignStatus.DRAFT
            : campaignStateData.Status == DOPPLER_CAMPAIGN_STATUS_IN_WINNER_IN_AB_SELECTION_PROCESS ? CampaignStatus.IN_WINNER_IN_AB_SELECTION_PROCESS
            : CampaignStatus.OTHER;

        return new CampaignState(
                campaignStateData.OwnCampaignExists,
                campaignStateData.ContentExists,
                campaignStateData.EditorType,
                campaignStatus
            );
    }

    public async Task CreateCampaignContent(string accounName, ContentData content)
    {

        IExecutableDbQuery insertContentQuery = content switch
        {
            UnlayerContentData unlayerContentData => new InsertCampaignContentDbQuery(
                IdCampaign: unlayerContentData.campaignId,
                Content: unlayerContentData.htmlContent,
                Head: unlayerContentData.htmlHead,
                Meta: unlayerContentData.meta,
                EditorType: (int?)EDITOR_TYPE_UNLAYER
            ),
            HtmlContentData htmlContentData => new InsertCampaignContentDbQuery(
                IdCampaign: htmlContentData.campaignId,
                Content: htmlContentData.htmlContent,
                Head: htmlContentData.htmlHead,
                Meta: null,
                EditorType: null
            ),
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {content.GetType()}")
        };

        await _dbContext.ExecuteAsync(insertContentQuery);

        var updateCampaignStatusQuery = new UpdateCampaignStatusDbQuery(
            setCurrentStep: 2,
            setHtmlSourceType: UpdateCampaignStatusDbQuery.TEMPLATE_HTML_SOURCE_TYPE,
            whenIdCampaignIs: content.campaignId,
            whenCurrentStepIs: 1
        );

        await _dbContext.ExecuteAsync(updateCampaignStatusQuery);
    }

    public async Task UpdateCampaignContent(string accounName, ContentData content)
    {
        IExecutableDbQuery updateContentQuery = content switch
        {
            UnlayerContentData unlayerContentData => new UpdateCampaignContentDbQuery(
                IdCampaign: unlayerContentData.campaignId,
                Content: unlayerContentData.htmlContent,
                Head: unlayerContentData.htmlHead,
                Meta: unlayerContentData.meta,
                EditorType: (int?)EDITOR_TYPE_UNLAYER
            ),
            HtmlContentData htmlContentData => new UpdateCampaignContentDbQuery(
                IdCampaign: htmlContentData.campaignId,
                Content: htmlContentData.htmlContent,
                Head: htmlContentData.htmlHead,
                Meta: null,
                EditorType: null
            ),
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {content.GetType()}")
        };

        await _dbContext.ExecuteAsync(updateContentQuery);
    }

    public async Task SaveNewFieldIds(int ContentId, IEnumerable<int> fieldsId)
    {
        if (!fieldsId.Any())
        {
            return;
        }

        await _dbContext.ExecuteAsync(new SaveNewCampaignFields(ContentId, fieldsId));
    }

    public async Task SaveLinks(int ContentId, IEnumerable<string> links)
    {
        if (links.Any())
        {
            await _dbContext.ExecuteAsync(new SaveNewCampaignLinks(ContentId, links));
        }
        await _dbContext.ExecuteAsync(new DeleteAutomationConditionalsOfRemovedCampaignLinks(ContentId, links));
        await _dbContext.ExecuteAsync(new DeleteRemovedCampaignLinks(ContentId, links));
    }
}
