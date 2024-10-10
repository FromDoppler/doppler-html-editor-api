using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdatePromoCodeDbQuery(
    int Id,
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    bool AllowCombines,
    decimal? MinPrice,
    int ExpireDays,
    string? Categories
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
    Categories = @Categories
WHERE IdDynamicContentPromoCode = @Id";
}
