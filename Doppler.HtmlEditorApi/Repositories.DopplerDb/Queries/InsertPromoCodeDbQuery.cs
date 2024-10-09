using Doppler.HtmlEditorApi.DataAccess;
using System;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record InsertPromoCodeDbQuery(
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    decimal? MinPrice,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Categories
) : ISingleItemDbQuery<InsertPromoCodeDbQuery.Result>
{
    public string GenerateSqlQuery() => @"
INSERT INTO DynamicContentPromoCode (
    Type,
    Value,
    IncludeShipping,
    FirstPurchase,
    MinPrice,
    StartDate,
    EndDate,
    Categories
) VALUES (
    @Type,
    @Value,
    @IncludeShipping,
    @FirstPurchase,
    @MinPrice,
    @StartDate,
    @EndDate,
    @Categories
)

SELECT @@Identity AS IdDynamicContentPromoCode";

    public class Result
    {
        public int IdDynamicContentPromoCode { get; init; }
    }
}
