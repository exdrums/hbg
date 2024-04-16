import DataSource from "devextreme/data/data_source";
import CustomStore from "devextreme/data/custom_store";
import { CustomDataStore } from "./custom-data-store";

export abstract class CustomDataSource<TGET = any, TKEY = number, TPOST = TGET> extends DataSource<TGET, TKEY> {
    constructor(protected customStore: CustomDataStore<TGET, TKEY>) {
        super({ store: customStore, reshapeOnPush: true });
    }
}