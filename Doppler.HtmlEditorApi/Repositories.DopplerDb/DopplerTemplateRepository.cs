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

    public async Task<TemplateModel> GetOwnOrPublicTemplate(string accountName, int templateId)
    {
        var queryResult = await _dbContext.ExecuteAsync(new GetTemplateByIdWithStatusDbQuery(
            IdTemplate: templateId,
            AccountName: accountName
        ));

        if (queryResult == null)
        {
            return null;
        }

        TemplateContentData content = queryResult.EditorType == EditorTypeUnlayer
            ? new UnlayerTemplateContentData(
                HtmlComplete: queryResult.HtmlCode,
                Meta: queryResult.Meta)
            : new UnknownTemplateContentData(
                EditorType: queryResult.EditorType);

        return new TemplateModel(templateId, queryResult.IsPublic, queryResult.PreviewImage, queryResult.Name, content);
    }

    public Task UpdateTemplate(TemplateModel templateModel) => throw new NotImplementedException();
}
