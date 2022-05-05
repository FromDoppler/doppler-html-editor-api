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
    private const int EditorTypeMSEditor = 4;
    private const int EditorTypeUnlayer = 5;
    private const int DopplerCampaignStatusDraft = 1;
    private const int DopplerCampaignStatusABDraft = 11;
    private const int DopplerCampaignStatusInWinnerInABSelectionProcess = 18;

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

        return queryResult == null || !queryResult.CampaignExists ? null
            : !queryResult.CampaignHasContent ? new EmptyContentData(campaignId)
            : queryResult.EditorType == EditorTypeMSEditor ? new MSEditorContentData(campaignId, queryResult.Content)
            : queryResult.EditorType == EditorTypeUnlayer ? new UnlayerContentData(
                CampaignId: queryResult.IdCampaign,
                HtmlContent: queryResult.Content,
                HtmlHead: queryResult.Head,
                Meta: queryResult.Meta,
                PreviewImage: queryResult.PreviewImage)
            : queryResult.EditorType == null ? new HtmlContentData(
                CampaignId: queryResult.IdCampaign,
                HtmlContent: queryResult.Content,
                HtmlHead: queryResult.Head,
                PreviewImage: queryResult.PreviewImage)
            : new UnknownContentData(
                CampaignId: queryResult.IdCampaign,
                Content: queryResult.Content,
                Head: queryResult.Head,
                Meta: queryResult.Meta,
                EditorType: queryResult.EditorType,
                PreviewImage: queryResult.PreviewImage);
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
        var campaignStatus = campaignStateData.Status is DopplerCampaignStatusDraft or
            DopplerCampaignStatusABDraft ? CampaignStatus.Draft
            : campaignStateData.Status == DopplerCampaignStatusInWinnerInABSelectionProcess ? CampaignStatus.InWinnerInABSelectionProcess
            : CampaignStatus.Other;

        return new CampaignState(
                campaignStateData.OwnCampaignExists,
                campaignStateData.ContentExists,
                campaignStateData.EditorType,
                campaignStatus
            );
    }

    public async Task CreateCampaignContent(string accountName, ContentData content)
    {

        IExecutableDbQuery insertContentQuery = content switch
        {
            UnlayerContentData unlayerContentData => new InsertCampaignContentDbQuery(
                IdCampaign: unlayerContentData.CampaignId,
                Content: unlayerContentData.HtmlContent,
                Head: unlayerContentData.HtmlHead,
                Meta: unlayerContentData.Meta,
                EditorType: (int?)EditorTypeUnlayer
            ),
            HtmlContentData htmlContentData => new InsertCampaignContentDbQuery(
                IdCampaign: htmlContentData.CampaignId,
                Content: htmlContentData.HtmlContent,
                Head: htmlContentData.HtmlHead,
                Meta: null,
                EditorType: null
            ),
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {content.GetType()}")
        };

        await _dbContext.ExecuteAsync(insertContentQuery);

        if (content is BaseHtmlContentData baseHtmlContentData)
        {
            var updatePreviewImageQuery = new UpdateCampaignPreviewImageDbQuery(
                baseHtmlContentData.CampaignId,
                baseHtmlContentData.PreviewImage);
            await _dbContext.ExecuteAsync(updatePreviewImageQuery);
        }

        var updateCampaignStatusQuery = new UpdateCampaignStatusDbQuery(
            SetCurrentStep: 2,
            SetHtmlSourceType: UpdateCampaignStatusDbQuery.TemplateHtmlSourceType,
            WhenIdCampaignIs: content.CampaignId,
            WhenCurrentStepIs: 1
        );

        await _dbContext.ExecuteAsync(updateCampaignStatusQuery);
    }

    public async Task UpdateCampaignContent(string accountName, ContentData content)
    {
        IExecutableDbQuery updateContentQuery = content switch
        {
            UnlayerContentData unlayerContentData => new UpdateCampaignContentDbQuery(
                IdCampaign: unlayerContentData.CampaignId,
                Content: unlayerContentData.HtmlContent,
                Head: unlayerContentData.HtmlHead,
                Meta: unlayerContentData.Meta,
                EditorType: (int?)EditorTypeUnlayer
            ),
            HtmlContentData htmlContentData => new UpdateCampaignContentDbQuery(
                IdCampaign: htmlContentData.CampaignId,
                Content: htmlContentData.HtmlContent,
                Head: htmlContentData.HtmlHead,
                Meta: null,
                EditorType: null
            ),
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {content.GetType()}")
        };

        await _dbContext.ExecuteAsync(updateContentQuery);

        if (content is BaseHtmlContentData baseHtmlContentData)
        {
            var updatePreviewImageQuery = new UpdateCampaignPreviewImageDbQuery(
                baseHtmlContentData.CampaignId,
                baseHtmlContentData.PreviewImage);
            await _dbContext.ExecuteAsync(updatePreviewImageQuery);
        }
    }

    public async Task SaveNewFieldIds(int contentId, IEnumerable<int> fieldsId)
    {
        if (!fieldsId.Any())
        {
            return;
        }

        await _dbContext.ExecuteAsync(new SaveNewCampaignFields(contentId, fieldsId));
    }

    public async Task SaveLinks(int contentId, IEnumerable<string> links)
    {
        if (links.Any())
        {
            await _dbContext.ExecuteAsync(new SaveNewCampaignLinks(contentId, links));
        }
        await _dbContext.ExecuteAsync(new DeleteAutomationConditionalsOfRemovedCampaignLinks(contentId, links));
        await _dbContext.ExecuteAsync(new DeleteRemovedCampaignLinks(contentId, links));
    }
}
