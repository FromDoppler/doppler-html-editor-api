using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage.DapperProvider.Queries;

public class UpdateCampaignStatusDbQuery : DbQuery<UpdateCampaignStatusDbQuery.Parameters, int>
{
    // See https://github.com/MakingSense/Doppler/blob/48cf637bb1f8b4d81837fff904d8736fe889ff1c/Doppler.Transversal/Classes/CampaignHTMLContentTypeEnum.cs#L12-L17
    // We are using Template because Editor seems to be tied to the old HTML Editor
    public const int TEMPLATE_HTML_SOURCE_TYPE = 2;

    public UpdateCampaignStatusDbQuery(IDbContext dbContext) : base(dbContext) { }

    protected override string SqlQuery => @"
UPDATE Campaign
SET
    CurrentStep = @setCurrentStep,
    HtmlSourceType = @setHtmlSourceType
WHERE
    IdCampaign = @whenIdCampaignIs
    AND CurrentStep = @whenCurrentStepIs";

    public override Task<int> ExecuteAsync(UpdateCampaignStatusDbQuery.Parameters parameters)
        => DbContext.ExecuteAsync(SqlQuery, parameters);

    public record Parameters(
        int setCurrentStep,
        int? setHtmlSourceType,
        int whenIdCampaignIs,
        int whenCurrentStepIs
    );
}
