using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record InsertPromoCodeDbQuery(
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    bool AllowCombines,
    decimal MinPrice,
    int ExpireDays,
    int MaxUses,
    string? Categories,
    int IdCampaign,
    string Prefix,
    string ThirdPartyApp
) : ISingleItemDbQuery<InsertPromoCodeDbQuery.Result>
{
    public string GenerateSqlQuery() => @"
DECLARE @IdThirdPartyApp INT;
SELECT @IdThirdPartyApp = IdThirdPartyApp FROM ThirdPartyApp WHERE Name = @ThirdPartyApp;

INSERT INTO DynamicContentPromoCode (
    Type,
    Value,
    IncludeShipping,
    FirstPurchase,
    AllowCombines,
    MinPrice,
    ExpireDays,
    MaxUses,
    Categories,
    IdCampaign,
    Prefix,
    IdThirdPartyApp
) VALUES (
    @Type,
    @Value,
    @IncludeShipping,
    @FirstPurchase,
    @AllowCombines,
    @MinPrice,
    @ExpireDays,
    @MaxUses,
    @Categories,
    @IdCampaign,
    @Prefix,
    ISNULL(@IdThirdPartyApp, 3)
)

SELECT @@Identity AS IdDynamicContentPromoCode";

    public class Result
    {
        public int IdDynamicContentPromoCode { get; init; }
    }
}
