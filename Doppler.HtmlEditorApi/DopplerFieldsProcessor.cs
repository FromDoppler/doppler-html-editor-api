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
    private readonly ReadOnlyDictionary<string, int> FieldIdsByNameOrAlias;
    private readonly ReadOnlyDictionary<int, string> FieldNamesById;

    public DopplerFieldsProcessor(IEnumerable<Field> fields, IEnumerable<FieldAliasesDef> aliasesByCanonical)
    {
        FieldIdsByNameOrAlias = CreateDictionaryOfIdsByNameOrAlias(fields, aliasesByCanonical);

        // Only canonical names
        FieldNamesById = new ReadOnlyDictionary<int, string>(
            fields.ToDictionary(x => x.id, x => x.name));
    }

    private static ReadOnlyDictionary<string, int> CreateDictionaryOfIdsByNameOrAlias(IEnumerable<Field> fields, IEnumerable<FieldAliasesDef> aliasesByCanonical)
    {
        var fieldIdsByNameOrAlias = fields.ToDictionary(x => x.name, x => x.id, StringComparer.OrdinalIgnoreCase);

        var idsAndAlias = aliasesByCanonical
            .SelectMany(x => x.aliases.Select(alias => new { x.canonicalName, alias }))
            .Where(x => fieldIdsByNameOrAlias.ContainsKey(x.canonicalName))
            .Select(x => new { id = fieldIdsByNameOrAlias[x.canonicalName], x.alias });

        foreach (var pair in idsAndAlias)
        {
            fieldIdsByNameOrAlias.TryAdd(pair.alias, pair.id);
        }

        return new ReadOnlyDictionary<string, int>(fieldIdsByNameOrAlias);
    }

    public int? GetFieldIdOrNull(string fieldName)
        => FieldIdsByNameOrAlias.TryGetValue(fieldName, out var fieldId) ? fieldId : null;

    public bool FieldIdExist(int fieldId)
        => FieldNamesById.ContainsKey(fieldId);
}
