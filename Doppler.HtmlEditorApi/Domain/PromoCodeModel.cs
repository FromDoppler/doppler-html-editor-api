using System;

namespace Doppler.HtmlEditorApi.Domain;

public record PromoCodeModel(
    int Id,
    string Type,
    decimal Value,
    bool IncludeShipping,
    bool FirstPurchase,
    bool CombineDiscounts,
    int ExpireDays,
    decimal? MinPrice,
    string Categories
    );

