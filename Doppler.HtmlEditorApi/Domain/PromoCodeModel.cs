using System;

namespace Doppler.HtmlEditorApi.Domain;

public record PromoCodeModel(
    int Id,
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    decimal? MinPrice,
    DateTime? StartDate,
    DateTime? EndDate,
    string Categories
    );

