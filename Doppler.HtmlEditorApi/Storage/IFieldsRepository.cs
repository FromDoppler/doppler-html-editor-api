using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage;

public interface IFieldsRepository
{
    Task<IEnumerable<Field>> GetActiveBasicFields();
    Task<IEnumerable<Field>> GetCustomFields(string accountname);

    /// <summary>
    /// It keeps existing DB entries and only adds new ones without deleting anything.
    /// </summary>
    Task SaveNewFieldIds(int accountName, IEnumerable<int> fieldsId);
}
