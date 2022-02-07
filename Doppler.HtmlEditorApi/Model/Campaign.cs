using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.Model;

public record CampaignContent(
    [Required]
    ContentType type,
    JsonElement? meta,
    [Required]
    string htmlContent) : Content(type, meta, htmlContent), IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (type == (ContentType)0)
        {
            yield return new ValidationResult($"The {nameof(type)} field is required.", new[] { nameof(type) });
        }
        else if (type != ContentType.unlayer)
        {
            yield return new ValidationResult($"Content type '{type:G}' is not supported yet.");
        }

        if (type == ContentType.unlayer && (meta == null || string.IsNullOrWhiteSpace(meta.ToString())))
        {
            yield return new ValidationResult($"The {nameof(meta)} field is required for unlayer content.", new[] { nameof(meta) });
        }

        yield break;
    }
}
