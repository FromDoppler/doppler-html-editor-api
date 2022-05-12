using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdateCampaignStatusDbQuery(
    int SetCurrentStep,
    int? SetHtmlSourceType,
    int WhenIdCampaignIs,
    int WhenCurrentStepIs
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
UPDATE Campaign
SET
    CurrentStep = @setCurrentStep,
    HtmlSourceType = @setHtmlSourceType
WHERE
    IdCampaign = @whenIdCampaignIs
    AND CurrentStep = @whenCurrentStepIs";

}
