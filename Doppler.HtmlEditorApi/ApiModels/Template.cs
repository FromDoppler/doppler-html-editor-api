using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Doppler.HtmlEditorApi.ApiModels;

public record Template(
    [Required]
    ContentType type,
    [Required]
    string templateName,
    // readonly
    bool isPublic,
    string previewImage,
    [Required]
    string htmlContent,
    [Required]
    JsonElement? meta) : Content(type, meta, htmlContent), IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (type == ContentType.unset)
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
