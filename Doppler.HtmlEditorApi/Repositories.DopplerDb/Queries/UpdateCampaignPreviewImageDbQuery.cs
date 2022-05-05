using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdateCampaignPreviewImageDbQuery(
    int IdCampaign,
    string PreviewImage
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
UPDATE Campaign
SET
    PreviewImage = @PreviewImage
WHERE IdCampaign = @IdCampaign";
}
