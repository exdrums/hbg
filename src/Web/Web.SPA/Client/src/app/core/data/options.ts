import { GroupItem } from "devextreme/data/custom_store";

export interface RestDataStoreUrls {
    key: string
    loadUrl?: string;
    insertUrl?: string;
    updateUrl?: string;
    removeUrl?: string;
    parentID?: number;
    // use PUT method for insert action for the datasource
    usePutForInsert?: boolean;
}

export type LoadedData<TItem = any> = {
    data: Array<TItem> | Array<GroupItem>;
    totalCount?: number;
    summary?: Array<any>;
    groupCount?: number;
};