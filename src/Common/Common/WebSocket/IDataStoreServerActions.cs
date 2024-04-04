using Common.Utils;

namespace Common.WebSocket;

public interface IDataStoreServerActions<T>
{
    Task Load(DevExtremeLoadOptions loadOptions);
    Task Insert(T value);
    Task Update(object key, object values);
    Task Remove(object key);
}
