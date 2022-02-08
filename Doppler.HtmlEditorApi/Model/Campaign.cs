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
    private static HashSet<ContentType> _validContentTypes = new HashSet<ContentType>(Enum.GetValues<ContentType>());

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (type == (ContentType)0)
        {
            yield return new ValidationResult($"The {nameof(type)} field is required.", new[] { nameof(type) });
        }
        else if (!_validContentTypes.Contains(type))
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
