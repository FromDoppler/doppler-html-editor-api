using System;
using System.Threading.Tasks;
using Dapper;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider;

public class Repository : IRepository
{
    // TODO: add types to represent Dapper queries results.
    // Because dynamic Dapper results do nowork with Moq.Dapper.

    private const int EDITOR_TYPE_MSEDITOR = 4;
    private const int EDITOR_TYPE_UNLAYER = 5;

    private readonly IDatabaseConnectionFactory _connectionFactory;
    public Repository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ContentData> GetCampaignModel(string accountName, int campaignId)
    {
        using (var connection = _connectionFactory.GetConnection())
        {
            var databaseQuery = @"
SELECT
CAST (CASE WHEN co.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS CampaignHasContent,
CAST (CASE WHEN ca.IdUser IS NULL THEN 0 ELSE 1 END AS BIT) AS CampaignBelongsUser,
CAST (CASE WHEN ca.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS CampaignExists,
ca.IdCampaign, co.Content, co.EditorType, co.Meta
FROM [User] u
LEFT JOIN [Campaign] ca ON u.IdUser = ca.IdUser
AND ca.IdCampaign = @IdCampaign
LEFT JOIN [Content] co ON ca.IdCampaign = co.IdCampaign
WHERE u.Email = @accountName";

            // TODO: use a type for the result
            var queryResult = await connection.QueryFirstOrDefaultAsync<dynamic>(databaseQuery, new { IdCampaign = campaignId, accountName });

            // TODO: test these both scenarios
            // Related tests:
            // * GET_campaign_should_accept_right_tokens_and_return_404_when_DB_returns_null
            if (!queryResult.CampaignBelongsUser || !queryResult.CampaignExists)
            {
                return null;
            }

            // TODO: test this scenario
            // Related tests:
            // * GET_campaign_should_return_empty_content_as_unlayer_content
            if (!queryResult.CampaignHasContent)
            {
                return new EmptyContentData(campaignId);
            };

            // TODO: test this scenario
            if (queryResult.EditorType == EDITOR_TYPE_MSEDITOR)
            {
                return new MSEditorContentData(campaignId, queryResult.Content);
            }

            // TODO: test this scenario
            // Related tests:
            // * GET_campaign_should_accept_right_tokens_and_return_unlayer_content
            if (queryResult.EditorType == EDITOR_TYPE_UNLAYER)
            {
                return new UnlayerContentData(
                    campaignId: queryResult.IdCampaign,
                    htmlContent: queryResult.Content,
                    meta: queryResult.Meta);
            }

            // TODO: test this scenario
            // Related tests:
            // * GET_campaign_should_return_html_content
            if (queryResult.EditorType == null)
            {
                return new HtmlContentData(
                    campaignId: queryResult.IdCampaign,
                    htmlContent: queryResult.Content);
            }

            // TODO: test this scenario
            return new UnknownContentData(
                campaignId: queryResult.IdCampaign,
                content: queryResult.Content,
                meta: queryResult.Meta,
                editorType: queryResult.EditorType);
        }
    }

    public async Task SaveCampaignContent(string accountName, ContentData contentRow)
    {
        using (var connection = _connectionFactory.GetConnection())
        {
            var databaseQuery = @"
SELECT
CAST (CASE WHEN ca.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS OwnCampaignExists,
CAST (CASE WHEN co.IdCampaign IS NULL THEN 0 ELSE 1 END AS BIT) AS ContentExists,
co.EditorType
FROM [User] u
LEFT JOIN [Campaign] ca ON u.IdUser = ca.IdUser
AND ca.IdCampaign = @IdCampaign
LEFT JOIN [Content] co ON ca.IdCampaign = co.IdCampaign
WHERE u.Email = @accountName
";
            // TODO: use a type for the result
            var campaignStatus = await connection.QueryFirstOrDefaultAsync<dynamic>(databaseQuery, new { contentRow.campaignId, accountName });

            // TODO: consider returning 404 NotFound
            // TODO: test this scenario
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

            await connection.ExecuteAsync(query, queryParams);
        }
    }
}
