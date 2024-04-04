export interface IDataStoreClientActions<T> {
    loaded: (list: T[]) => void;
    added: (item: T) => void;
}