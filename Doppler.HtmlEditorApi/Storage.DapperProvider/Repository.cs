using System;
using System.Threading.Tasks;
using Dapper;
using Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public class Repository : IRepository
{
    private const int EDITOR_TYPE_MSEDITOR = 4;
    private const int EDITOR_TYPE_UNLAYER = 5;

    private readonly IDbContext _dbContext;
    public Repository(IDbContext dbContext)
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

    public async Task SaveCampaignContent(string accountName, ContentData contentRow)
    {
        var campaignStatus = await _dbContext.ExecuteAsync(new FirstOrDefaultCampaignStatusDbQuery(
            AccountName: accountName,
            IdCampaign: contentRow.campaignId
        ));

        // TODO: consider returning 404 NotFound
        if (campaignStatus == null || !campaignStatus.OwnCampaignExists)
        {
            throw new ApplicationException($"CampaignId {contentRow.campaignId} does not exists or belongs to another user than {accountName}");
        }

        var queryParams = contentRow switch
        {
            UnlayerContentData unlayerContentData => new ContentRow()
            {
                IdCampaign = unlayerContentData.campaignId,
                Content = unlayerContentData.htmlContent,
                Head = unlayerContentData.htmlHead,
                Meta = unlayerContentData.meta,
                EditorType = (int?)EDITOR_TYPE_UNLAYER
            },
            HtmlContentData htmlContentData => new ContentRow()
            {
                IdCampaign = htmlContentData.campaignId,
                Content = htmlContentData.htmlContent,
                Head = htmlContentData.htmlHead,
                Meta = (string)null,
                EditorType = (int?)null
            },
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {contentRow.GetType()}")
        };

        IExecutableDbQuery upsertContentQuery = campaignStatus.ContentExists
            ? new UpdateCampaignContentDbQuery(queryParams)
            : new InsertCampaignContentDbQuery(queryParams);

        await _dbContext.ExecuteAsync(upsertContentQuery);

        var updateCampaignStatusQuery = new UpdateCampaignStatusDbQuery(
            setCurrentStep: 2,
            setHtmlSourceType: UpdateCampaignStatusDbQuery.TEMPLATE_HTML_SOURCE_TYPE,
            whenIdCampaignIs: contentRow.campaignId,
            whenCurrentStepIs: 1
        );

        await _dbContext.ExecuteAsync(updateCampaignStatusQuery);
    }
}
