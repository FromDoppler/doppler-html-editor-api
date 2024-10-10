using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Doppler.HtmlEditorApi.ApiModels;

public record PromoCode(
    [Required]
        string type,
    [Required]
        decimal value,
    [Required]
        bool includeShipping,
    [Required]
        bool firstPurchase,
    bool combineDiscounts,
    int expireDays,
    decimal minPrice,
    int maxUses,
    string cagetories
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) { yield break; }
}

