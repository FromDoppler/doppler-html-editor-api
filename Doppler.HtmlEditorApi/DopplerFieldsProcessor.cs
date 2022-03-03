using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Doppler.HtmlEditorApi;

/// <summary>
/// This class deals with the conversion between Doppler field-names
/// to Doppler field-ids and vice versa.
/// </summary>
public class DopplerFieldsProcessor
{
    private readonly ReadOnlyDictionary<int, string> _fieldNamesById;

    public DopplerFieldsProcessor(IEnumerable<Field> fields)
    {
        // TODO: receive and prepare fields information

        // Only canonical names
        _fieldNamesById = new ReadOnlyDictionary<int, string>(
            fields.ToDictionary(x => x.id, x => x.name));
    }

    public int? GetFieldIdOrNull(string fieldName)
        // TODO: complete the behavior here
        => null;

    public bool FieldIdExist(int fieldId)
        => _fieldNamesById.ContainsKey(fieldId);
}
