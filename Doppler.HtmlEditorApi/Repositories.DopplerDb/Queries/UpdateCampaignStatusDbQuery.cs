using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdateCampaignStatusDbQuery(
    int SetCurrentStep,
    int? SetHtmlSourceType,
    int WhenIdCampaignIs,
    int WhenCurrentStepIs
) : IExecutableDbQuery
{
    // See https://github.com/MakingSense/Doppler/blob/48cf637bb1f8b4d81837fff904d8736fe889ff1c/Doppler.Transversal/Classes/CampaignHTMLContentTypeEnum.cs#L12-L17
    // We are using Template because Editor seems to be tied to the old HTML Editor
    public const int TEMPLATE_HTML_SOURCE_TYPE = 2;

    public string GenerateSqlQuery() => @"
UPDATE Campaign
SET
    CurrentStep = @setCurrentStep,
    HtmlSourceType = @setHtmlSourceType
WHERE
    IdCampaign = @whenIdCampaignIs
    AND CurrentStep = @whenCurrentStepIs";

}
