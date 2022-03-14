using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.HtmlEditorApi.Storage;

public interface IFieldsRepository
{
    Task<IEnumerable<Field>> GetActiveBasicFields();
    Task<IEnumerable<Field>> GetCustomFields(string accountname);
    Task SaveFieldsId(int accountName, IEnumerable<int> fieldsId);
}
