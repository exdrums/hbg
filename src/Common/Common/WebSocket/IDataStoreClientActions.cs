namespace Common.WebSocket;
public interface IDataStoreClientActions<T>
{
    Task Loaded(List<T> list);
    Task Added(T item);
}
