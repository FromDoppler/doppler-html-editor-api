using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model;

public record CampaignContent(
    [Required]
    JsonElement meta,
    [Required]
    string htmlContent) : Content(meta, htmlContent), IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(meta.ToString()))
        {
            yield return new ValidationResult($"The {nameof(meta)} field is required.", new[] { nameof(meta) });
        }
        yield break;
    }
}

