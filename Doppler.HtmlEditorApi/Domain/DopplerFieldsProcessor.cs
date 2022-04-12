using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Doppler.HtmlEditorApi.Domain;

/// <summary>
/// This class deals with the conversion between Doppler field-names
/// to Doppler field-ids and vice versa.
/// </summary>
public class DopplerFieldsProcessor
{
    private readonly ReadOnlyDictionary<string, int> _fieldIdsByNameOrAlias;

    private readonly ReadOnlyDictionary<int, string> _fieldNamesById;

    public DopplerFieldsProcessor(IEnumerable<Field> fields, IEnumerable<FieldAliasesDef> aliasesByCanonical)
    {
        _fieldIdsByNameOrAlias = CreateDictionaryOfIdsByNameOrAlias(fields, aliasesByCanonical);

        // Only canonical names
        _fieldNamesById = new ReadOnlyDictionary<int, string>(
            fields.ToDictionary(x => x.Id, x => x.Name));
    }

    public int? GetFieldIdOrNull(string fieldName)
        => _fieldIdsByNameOrAlias.TryGetValue(fieldName, out var fieldId)
            ? fieldId
            : null;

    public string GetFieldNameOrNull(int fieldId)
        => _fieldNamesById.TryGetValue(fieldId, out var fieldName)
            ? fieldName
            : null;

    public bool FieldIdExist(int fieldId)
        => _fieldNamesById.ContainsKey(fieldId);

    private static ReadOnlyDictionary<string, int> CreateDictionaryOfIdsByNameOrAlias(IEnumerable<Field> fields, IEnumerable<FieldAliasesDef> aliasesByCanonical)
    {
        var fieldIdsByNameOrAlias = fields.ToDictionary(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase);

        var idsAndAlias = aliasesByCanonical
            .SelectMany(x => x.Aliases.Select(alias => new { x.CanonicalName, alias }))
            .Where(x => fieldIdsByNameOrAlias.ContainsKey(x.CanonicalName))
            .Select(x => new { id = fieldIdsByNameOrAlias[x.CanonicalName], x.alias });

        foreach (var pair in idsAndAlias)
        {
            fieldIdsByNameOrAlias.TryAdd(pair.alias, pair.id);
        }

        return new ReadOnlyDictionary<string, int>(fieldIdsByNameOrAlias);
    }
}
