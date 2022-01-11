using System;
using Doppler.HtmlEditorApi.Model;
using System.Threading.Tasks;
using Dapper;

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
                var res = JsonSerializer.Deserialize<ContentModel>(result.Meta);
                return res == null ? null : res;
            }
        }

        public async Task<TemplateModel> GetTemplateModel(string accountName, int templateId)
        {
            return null;
        }

        public async Task<TemplateModel> GetSharedTemplateModel(int templateId)
        {
            return null;
        }

        public Task SaveCampaignContent(string accountName, int campaignId, ContentModel campaignModel)
        {
            return Task.CompletedTask;
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
