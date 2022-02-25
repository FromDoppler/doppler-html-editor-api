using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Doppler.HtmlEditorApi;

/// <summary>
/// This class deals with the conversion between Doppler field-names
/// to Doppler field-ids and vice versa.
/// </summary>
public class DopplerFieldsProcessor
{
    public const string FIELD_START_DELIMITER = "[[[";
    public const string FIELD_END_DELIMITER = "]]]";
    public const string FIELD_START_DELIMITER_BACK_END = "|*|";
    public const string FIELD_END_DELIMITER_BACK_END = "*|*";
    public static readonly Regex BACKEND_FIELD_REGEX = new Regex($@"{Regex.Escape(FIELD_START_DELIMITER_BACK_END)}(\d+){Regex.Escape(FIELD_END_DELIMITER_BACK_END)}");

    // % is here to accept %20
    public static readonly Regex FIELD_REGEX = new Regex($@"{Regex.Escape(FIELD_START_DELIMITER)}([a-zA-Z0-9 \-_ñÑáéíóúÁÉÍÓÚ%]+){Regex.Escape(FIELD_END_DELIMITER)}");

    private readonly ReadOnlyDictionary<string, int> FieldIdsByNameOrAlias;
    private readonly ReadOnlyDictionary<int, string> FieldNamesById;

    public DopplerFieldsProcessor(IEnumerable<Field> fields)
    {
        // TODO: support field aliases and %20 in place of spaces
        FieldIdsByNameOrAlias = new ReadOnlyDictionary<string, int>(
            fields.ToDictionary(x => x.name, x => x.id, StringComparer.OrdinalIgnoreCase));

        // Only canonical names
        FieldNamesById = new ReadOnlyDictionary<int, string>(
            fields.ToDictionary(x => x.id, x => x.name));
    }

    public string ReplaceFieldNamesToFieldIdsInHtmlContent(string inputHtml)
        => FIELD_REGEX.Replace(
            inputHtml,
            match => FieldIdsByNameOrAlias.TryGetValue(match.Groups[1].Value, out var fieldId)
                ? $"{FIELD_START_DELIMITER_BACK_END}{fieldId}{FIELD_END_DELIMITER_BACK_END}"
                : match.Value);

    public string ClearInexistentFieldIs(string inputHtml)
        => BACKEND_FIELD_REGEX.Replace(
            inputHtml,
            match => FieldIdExist(int.Parse(match.Groups[1].Value)) ? match.Value : string.Empty);

    private bool FieldIdExist(int fieldId)
        // TODO: use field information to decide it
        => FieldNamesById.ContainsKey(fieldId);
}
