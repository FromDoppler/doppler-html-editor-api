using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdatePromoCodeDbQuery(
    int Id,
    int IdCampaign,
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    bool AllowCombines,
    decimal MinPrice,
    int ExpireDays,
    int MaxUses,
    string? Categories,
    string Prefix,
    string ThirdPartyApp
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
DECLARE @IdThirdPartyApp INT;
SELECT @IdThirdPartyApp = IdThirdPartyApp FROM ThirdPartyApp WHERE Name = @ThirdPartyApp;

UPDATE DynamicContentPromoCode
SET Type = @Type,
    Value = @Value,
    IncludeShipping = @IncludeShipping,
    FirstPurchase = @FirstPurchase,
    AllowCombines = @AllowCombines,
    MinPrice = @MinPrice,
    ExpireDays = @ExpireDays,
    MaxUses = @MaxUses,
    Categories = @Categories,
    Prefix = @Prefix,
    IdThirdPartyApp = ISNULL(@IdThirdPartyApp, IdThirdPartyApp)
WHERE IdDynamicContentPromoCode = @Id AND IdCampaign = @IdCampaign";
}
