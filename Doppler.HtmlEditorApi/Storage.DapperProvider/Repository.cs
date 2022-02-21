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
        var queryResult = await new FirstOrDefaultContentWithCampaignStatusDbQuery(_dbContext)
            .ExecuteAsync(new()
            {
                IdCampaign = campaignId,
                AccountName = accountName
            });

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
                meta: queryResult.Meta);
        }

        if (queryResult.EditorType == null)
        {
            return new HtmlContentData(
                campaignId: queryResult.IdCampaign,
                htmlContent: queryResult.Content);
        }

        return new UnknownContentData(
            campaignId: queryResult.IdCampaign,
            content: queryResult.Content,
            meta: queryResult.Meta,
            editorType: queryResult.EditorType);
    }

    public async Task SaveCampaignContent(string accountName, ContentData contentRow)
    {
        var campaignStatus = await new FirstOrDefaultCampaignStatusDbQuery(_dbContext)
            .ExecuteAsync(new()
            {
                AccountName = accountName,
                IdCampaign = contentRow.campaignId
            });

        // TODO: consider returning 404 NotFound
        // TODO: test this scenario
        // TODO: take into account when the user does not exist
        if (!campaignStatus.OwnCampaignExists)
        {
            throw new ApplicationException($"CampaignId {contentRow.campaignId} does not exists or belongs to another user than {accountName}");
        }

        // TODO: test these scenarios
        var query = campaignStatus.ContentExists
            ? @"UPDATE Content SET Content = @Content, Meta = @Meta, EditorType = @EditorType WHERE IdCampaign = @IdCampaign"
            : @"INSERT INTO Content (IdCampaign, Content, Meta, EditorType) VALUES (@IdCampaign, @Content, @Meta, @EditorType)";

        var queryParams = contentRow switch
        {
            // TODO: test this scenario
            // Related tests:
            // * PUT_campaign_should_store_unlayer_content
            UnlayerContentData unlayerContentData => new
            {
                IdCampaign = unlayerContentData.campaignId,
                Content = unlayerContentData.htmlContent,
                Meta = unlayerContentData.meta,
                EditorType = (int?)EDITOR_TYPE_UNLAYER
            },
            // TODO: test this scenario
            // Related tests:
            // * PUT_campaign_should_store_html_content
            HtmlContentData htmlContentData => new
            {
                IdCampaign = htmlContentData.campaignId,
                Content = htmlContentData.htmlContent,
                Meta = (string)null,
                EditorType = (int?)null
            },
            // TODO: test this scenario
            // Probably a unit test will be necessary
            _ => throw new NotImplementedException($"Unsupported campaign content type {contentRow.GetType()}")
        };

        await _dbContext.ExecuteAsync(query, queryParams);
    }
}
