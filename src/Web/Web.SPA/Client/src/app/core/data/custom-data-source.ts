import DataSource from "devextreme/data/data_source";
import CustomStore from "devextreme/data/custom_store";
import { CustomDataStore } from "./custom-data-store";
import { BehaviorSubject } from "rxjs";

export abstract class CustomDataSource<TGET = any, TKEY = number, TPOST = TGET> extends DataSource<TGET, TKEY> {
    constructor(protected customStore: CustomDataStore<TGET, TKEY>) {
        super({ store: customStore, reshapeOnPush: true });
    }

    public readonly selected$ = new BehaviorSubject<TGET>(undefined);
    public readonly selected = () => this.selected$.value;
    public readonly setSelected = (model: TGET) => this.selected$.next(model);

}