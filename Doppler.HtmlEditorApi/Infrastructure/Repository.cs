using System;
using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;
using Dapper;
using System.Text.Json;
using System.Collections.Generic;

namespace Doppler.HtmlEditorApi.Infrastructure
{
    public class Repository : IRepository
    {
        private readonly IDatabaseConnectionFactory _connectionFactory;
        public Repository(IDatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<ContentRow> GetCampaignModel(string accountName, int campaignId)
        {
            using (var connection = await _connectionFactory.GetConnection())
            {
                var databaseQuery = @"SELECT co.IdCampaign, co.Content, co.EditorType, co.Meta FROM Content co
JOIN Campaign ca ON ca.IdCampaign = co.IdCampaign
JOIN [User] u ON u.IdUser = ca.IdUser
WHERE co.IdCampaign = @campaignId  AND u.Email = @accountName AND co.EditorType = 5";
                return await connection.QueryFirstOrDefaultAsync<ContentRow>(databaseQuery, new { campaignId, accountName });
            }
        }

        public async Task SaveCampaignContent(string accountName, int campaignId, ContentRow contentRow)
        {
            using (var connection = await _connectionFactory.GetConnection())
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
                var campaignStatus = await connection.QueryFirstOrDefaultAsync<dynamic>(databaseQuery, new { IdCampaign = campaignId, accountName });

                if (!campaignStatus.OwnCampaignExists)
                {
                    throw new ApplicationException($"CampaignId {campaignId} does not exists or belongs to another user than {accountName}");
                }

                var query = campaignStatus.ContentExists
                    ? @"UPDATE Content SET Content = @Content, Meta = @Meta, EditorType = @EditorType WHERE IdCampaign = @IdCampaign"
                    : @"INSERT INTO Content (IdCampaign, Content, Meta, EditorType) VALUES (@IdCampaign, @Content, @Meta, @EditorType)";

                await connection.ExecuteAsync(query, contentRow);
            }
        }
    }
}
