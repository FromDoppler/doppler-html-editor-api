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
    string Prefix
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
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
    Prefix = @Prefix
WHERE IdDynamicContentPromoCode = @Id AND IdCampaign = @IdCampaign";
}
