import DataSource from "devextreme/data/data_source";
import CustomStore from "devextreme/data/custom_store";

export abstract class CustomDataSource<TGET = any, TKEY = number, TPOST = TGET> extends DataSource<TGET, TKEY> {
    constructor(store: CustomStore<TGET, TKEY>) {
        super({ store, reshapeOnPush: true });
    }
}