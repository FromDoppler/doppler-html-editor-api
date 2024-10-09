using Doppler.HtmlEditorApi.DataAccess;
using System;

namespace Doppler.HtmlEditorApi.Repositories.DopplerDb.Queries;

public record UpdatePromoCodeDbQuery(
    int Id,
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    decimal? MinPrice,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Categories
) : IExecutableDbQuery
{
    public string GenerateSqlQuery() => @"
UPDATE DynamicContentPromoCode
SET Type = @Type,
    Value = @Value,
    IncludeShipping = @IncludeShipping,
    FirstPurchase = @FirstPurchase,
    MinPrice = @MinPrice,
    StartDate = @StartDate,
    EndDate = @EndDate,
    Categories = @Categories
WHERE IdDynamicContentPromoCode = @Id";
}
