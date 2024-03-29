using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb;

public class DopplerCampaignContentRepository : ICampaignContentRepository
{
    private const int EditorTypeMSEditor = 4;
    private const int EditorTypeUnlayer = 5;
    private const int DopplerCampaignStatusDraft = 1;
    private const int DopplerCampaignStatusABDraft = 11;
    private const int DopplerCampaignStatusInWinnerInABSelectionProcess = 18;
    private const int DopplerCampaignTestTypeSubject = 1;
    private static int? DopplerCampaignTypeClassic => null;

    private readonly IDbContext _dbContext;
    public DopplerCampaignContentRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CampaignModel> GetCampaignModel(string accountName, int campaignId)
    {
        var queryResult = await _dbContext.ExecuteAsync(new FirstOrDefaultContentWithCampaignStatusDbQuery(
            IdCampaign: campaignId,
            AccountName: accountName
        ));

        if (queryResult == null || !queryResult.CampaignExists)
        {
            return null;
        }

        CampaignContentData content = !queryResult.CampaignHasContent ? new EmptyCampaignContentData()
            : queryResult.EditorType == EditorTypeMSEditor ? new MSEditorCampaignContentData(queryResult.Content)
            : queryResult.EditorType == EditorTypeUnlayer ? new UnlayerCampaignContentData(
                HtmlContent: queryResult.Content,
                HtmlHead: queryResult.Head,
                Meta: queryResult.Meta,
                IdTemplate: queryResult.IdTemplate)
            : queryResult.EditorType == null ? new HtmlCampaignContentData(
                HtmlContent: queryResult.Content,
                HtmlHead: queryResult.Head,
                IdTemplate: queryResult.IdTemplate)
            : new UnknownCampaignContentData(
                Content: queryResult.Content,
                Head: queryResult.Head,
                Meta: queryResult.Meta,
                EditorType: queryResult.EditorType);

        return new CampaignModel(
            queryResult.IdCampaign,
            Name: queryResult.Name,
            queryResult.PreviewImage,
            content
        );
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

        if (campaignStateData.TestType == DopplerCampaignTypeClassic)
        {
            return new ClassicCampaignState(campaignId, campaignStateData.ContentExists, campaignStateData.EditorType, campaignStatus);
        }

        var campaignTestABCondition = campaignStateData.TestType == DopplerCampaignTestTypeSubject
            ? TestABCondition.TypeTestABSubject
            : TestABCondition.TypeTestABContent;

        return new TestABCampaignState(
                campaignStateData.ContentExists,
                campaignStateData.EditorType,
                campaignStatus,
                campaignTestABCondition,
                campaignStateData.IdCampaignA,
                campaignStateData.IdCampaignB,
                campaignStateData.IdCampaignResult
            );
    }

    public async Task CreateCampaignContent(int campaignId, CampaignContentData content)
    {

        IExecutableDbQuery insertContentQuery = content switch
        {
            UnlayerCampaignContentData unlayerContentData => new InsertCampaignContentDbQuery(
                IdCampaign: campaignId,
                Content: unlayerContentData.HtmlContent,
                Head: unlayerContentData.HtmlHead,
                Meta: unlayerContentData.Meta,
                EditorType: EditorTypeUnlayer,
                IdTemplate: unlayerContentData.IdTemplate
            ),
            HtmlCampaignContentData htmlContentData => new InsertCampaignContentDbQuery(
                IdCampaign: campaignId,
                Content: htmlContentData.HtmlContent,
                Head: htmlContentData.HtmlHead,
                Meta: null,
                EditorType: null,
                IdTemplate: htmlContentData.IdTemplate
            ),
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {content.GetType()}")
        };

        await _dbContext.ExecuteAsync(insertContentQuery);
    }

    public async Task UpdateCampaignContent(int campaignId, CampaignContentData content)
    {
        IExecutableDbQuery updateContentQuery = content switch
        {
            UnlayerCampaignContentData unlayerContentData => new UpdateCampaignContentDbQuery(
                IdCampaign: campaignId,
                Content: unlayerContentData.HtmlContent,
                Head: unlayerContentData.HtmlHead,
                Meta: unlayerContentData.Meta,
                EditorType: EditorTypeUnlayer,
                IdTemplate: unlayerContentData.IdTemplate
            ),
            HtmlCampaignContentData htmlContentData => new UpdateCampaignContentDbQuery(
                IdCampaign: campaignId,
                Content: htmlContentData.HtmlContent,
                Head: htmlContentData.HtmlHead,
                Meta: null,
                EditorType: null,
                IdTemplate: htmlContentData.IdTemplate
            ),
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {content.GetType()}")
        };

        await _dbContext.ExecuteAsync(updateContentQuery);
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

    public async Task UpdateCampaignStatus(
        int setCurrentStep,
        int setHtmlSourceType,
        int setContentType,
        int whenIdCampaignIs,
        int whenCurrentStepIs)
    {
        var updateCampaignStatusQuery = new UpdateCampaignStatusDbQuery(
            setCurrentStep,
            setHtmlSourceType,
            setContentType,
            whenIdCampaignIs,
            whenCurrentStepIs);
        await _dbContext.ExecuteAsync(updateCampaignStatusQuery);
    }

    public async Task UpdateCampaignPreviewImage(int campaignId, string previewImage)
    {
        var updatePreviewImageQuery = new UpdateCampaignPreviewImageDbQuery(campaignId, previewImage);
        await _dbContext.ExecuteAsync(updatePreviewImageQuery);
    }
}
