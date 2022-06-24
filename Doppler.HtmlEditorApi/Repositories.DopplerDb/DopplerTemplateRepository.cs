using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.HtmlEditorApi.DataAccess;
using Doppler.HtmlEditorApi.Domain;
using Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb;

public class DopplerTemplateRepository : ITemplateRepository
{
    private const int EditorTypeUnlayer = 5;

    private readonly IDbContext _dbContext;
    public DopplerTemplateRepository(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TemplateData> GetTemplate(string accountName, int templateId)
    {
        var queryResult = await _dbContext.ExecuteAsync(new GetTemplateByIdWithStatusDbQuery(
            IdTemplate: templateId,
            AccountName: accountName
        ));

        return queryResult == null ? null
            : (queryResult.IsPublic && queryResult.EditorType == EditorTypeUnlayer) ? new UnlayerTemplateData(
                HtmlCode: queryResult.HtmlCode,
                Meta: queryResult.Meta,
                PreviewImage: queryResult.PreviewImage,
                EditorType: queryResult.EditorType,
                IsPublic: queryResult.IsPublic)
            // TODO: improve flow for content when is from other editor type
            : new UnknownTemplateData(
                HtmlCode: queryResult.HtmlCode,
                PreviewImage: queryResult.PreviewImage,
                EditorType: queryResult.EditorType,
                IsPublic: queryResult.IsPublic);
    }
}
