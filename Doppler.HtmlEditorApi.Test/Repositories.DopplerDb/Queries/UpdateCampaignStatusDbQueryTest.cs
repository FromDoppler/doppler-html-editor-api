using Xunit;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public class UpdateCampaignStatusDbQueryTest
{
    [Fact]
    public void UpdateCampaignStatusDbQuery_should_update_campaign_when_desired_step()
    {
        // Arrange
        var idCampaign = 567;
        var oldStep = 1;
        var newStep = 2;
        var newHtmlSourceType = 3;
        var newContentType = 4;

        var dbQuery = new UpdateCampaignStatusDbQuery(
            SetCurrentStep: newStep,
            SetHtmlSourceType: newHtmlSourceType,
            SetContentType: newContentType,
            WhenIdCampaignIs: idCampaign,
            WhenCurrentStepIs: oldStep
        );

        var expectedQuery = """
            UPDATE Campaign
            SET
                CurrentStep = @setCurrentStep,
                HtmlSourceType = @setHtmlSourceType,
                ContentType = @setContentType
            WHERE
                IdCampaign = @whenIdCampaignIs
                AND CurrentStep = @whenCurrentStepIs
            """;

        // Act
        var sqlQuery = dbQuery.GenerateSqlQuery();

        // Assert
        Assert.Equal(expectedQuery, sqlQuery);
        Assert.Equal(idCampaign, dbQuery.WhenIdCampaignIs);
        Assert.Equal(oldStep, dbQuery.WhenCurrentStepIs);
        Assert.Equal(newStep, dbQuery.SetCurrentStep);
        Assert.Equal(newHtmlSourceType, dbQuery.SetHtmlSourceType);
    }
}
