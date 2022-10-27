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

    public async Task<TemplateContentData> GetTemplate(string accountName, int templateId)
    {
        var queryResult = await _dbContext.ExecuteAsync(new GetTemplateByIdWithStatusDbQuery(
            IdTemplate: templateId,
            AccountName: accountName
        ));

        return queryResult == null ? null
            : queryResult.EditorType == EditorTypeUnlayer ? new UnlayerTemplateContentData(
                HtmlCode: queryResult.HtmlCode,
                Meta: queryResult.Meta,
                PreviewImage: queryResult.PreviewImage,
                Name: queryResult.Name,
                EditorType: queryResult.EditorType,
                IsPublic: queryResult.IsPublic)
            : new UnknownTemplateContentData(
                EditorType: queryResult.EditorType,
                IsPublic: queryResult.IsPublic);
    }
}
