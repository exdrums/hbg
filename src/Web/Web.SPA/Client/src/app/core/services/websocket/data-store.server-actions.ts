import DevExpress from "devextreme";

export interface IDataStoreServerActions<T> {
    load: (loadOptions: DevExpress.data.LoadOptions) => Promise<T[]>;
    insert: (value: T) => Promise<T>;
    update: (key, values) => Promise<void>;
    remove: (key) => Promise<void>;
}