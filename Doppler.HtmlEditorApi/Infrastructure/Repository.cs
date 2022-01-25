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

        public async Task<ContentModel> GetCampaignModel(string accountName, int campaignId)
        {
            using (var connection = await _connectionFactory.GetConnection())
            {
                var databaseQuery = @"SELECT co.IdCampaign, co.Content, co.EditorType, co.Meta FROM Content co
JOIN Campaign ca ON ca.IdCampaign = co.IdCampaign
JOIN [User] u ON u.IdUser = ca.IdUser
WHERE co.IdCampaign = @campaignId  AND u.Email = @accountName AND co.EditorType = 5";
                var result = await connection.QueryFirstOrDefaultAsync<ContentRow>(databaseQuery, new { campaignId, accountName });
                return result == null ? null : JsonSerializer.Deserialize<ContentModel>(result.Meta);
            }
        }

        public Task<TemplateModel> GetTemplateModel(string accountName, int templateId)
        {
            return Task.FromResult<TemplateModel>(null);
        }

        public Task<TemplateModel> GetSharedTemplateModel(int templateId)
        {
            return Task.FromResult<TemplateModel>(null);
        }

        public async Task<bool> SaveCampaignContent(string accountName, int campaignId, CampaignContentRequest request)
        {
            using (var connection = await _connectionFactory.GetConnection())
            {
                var databaseQuery = @"SELECT co.IdCampaign FROM Content co
JOIN Campaign ca ON ca.IdCampaign = co.IdCampaign
JOIN [User] u ON u.IdUser = ca.IdUser
WHERE co.IdCampaign = @campaignId  AND u.Email = @accountName AND co.EditorType = 5";
                var checkIfExistAndUserIsOwner = await connection.QueryFirstOrDefaultAsync<ContentRow>(databaseQuery, new { campaignId, accountName });
                // TODO: maybe need check if exist as other owner
                var databaseExec = @"INSERT INTO Content (IdCampaign, Content, Meta, EditorType) VALUES (@campaignId, @htmlContent, @metaModel, 5)";
                if (checkIfExistAndUserIsOwner != null)
                {
                    databaseExec = @"UPDATE Content SET Content = @htmlContent, Meta = @metaModel WHERE IdCampaign = @campaignId";
                }

                var metaModel = request.Meta.ToString();
                var result = await connection.ExecuteAsync(databaseExec, new { campaignId, htmlContent = request.Content, metaModel });
                return result > 0;
            }
        }

        public Task SaveTemplateContent(string accountName, int templateId, TemplateModel templateModel)
        {
            return Task.CompletedTask;
        }

        public Task CreateTemplate(string accountName, TemplateModel templateModel)
        {
            return Task.CompletedTask;
        }
    }
}
