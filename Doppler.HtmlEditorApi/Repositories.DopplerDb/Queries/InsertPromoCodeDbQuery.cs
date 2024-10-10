using Doppler.HtmlEditorApi.DataAccess;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record InsertPromoCodeDbQuery(
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    bool AllowCombines,
    decimal? MinPrice,
    int ExpireDays,
    string? Categories
) : ISingleItemDbQuery<InsertPromoCodeDbQuery.Result>
{
    public string GenerateSqlQuery() => @"
INSERT INTO DynamicContentPromoCode (
    Type,
    Value,
    IncludeShipping,
    FirstPurchase,
    AllowCombines,
    MinPrice,
    ExpireDays,
    Categories
) VALUES (
    @Type,
    @Value,
    @IncludeShipping,
    @FirstPurchase,
    @AllowCombines,
    @MinPrice,
    @ExpireDays,
    @Categories
)

SELECT @@Identity AS IdDynamicContentPromoCode";

    public class Result
    {
        public int IdDynamicContentPromoCode { get; init; }
    }
}
