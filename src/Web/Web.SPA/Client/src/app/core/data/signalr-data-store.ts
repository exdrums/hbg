import CustomStore from "devextreme/data/custom_store";
import DevExpress from "devextreme";
import { DataStoreWsConnection } from "../services/websocket/data-store-ws-connection";
import { delay, tap } from "rxjs";
import { DeepPartial } from "devextreme/core";


export class SignalRDataStore<TGET, TKEY = number, TPOST = TGET> extends CustomStore<TGET, TKEY>  {
    constructor(private connection: DataStoreWsConnection<TGET>, key: string) {
        super({
            key: key,
            loadMode: "raw",
            cacheRawData: true,
            load: (loadOptions: DevExpress.data.LoadOptions) => connection.load(loadOptions),
            insert: (value: TGET) => connection.insert(value),
            update: (key, values) => connection.update(key, values),
            remove: (key) => connection.remove(key),
            onPush: (changes) => console.log('onPush_changes', changes)
        });
        this.bindLoaded();
        this.bindAdded();
    }

    private bindLoaded = () => this.connection.loaded$.pipe(
        delay(0),
        tap(i => console.log('loaded$', i, this)),
        tap(items => this.push(items.map(i => ({ type: "insert", data: i as DeepPartial<TGET>, index: 0 })))),
    ).subscribe();

    private bindAdded = () => this.connection.added$.pipe(
        delay(0),
        tap(i => console.log('added$', i, this)),
        tap(added => this.push([{ type: "insert", data: added as DeepPartial<TGET> }]))
    ).subscribe();
    
}