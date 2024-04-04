import { BehaviorSubject, Observable, firstValueFrom } from "rxjs";
import { AuthService } from "../auth.service";
import { SignalRAction, WsConnection } from "./ws-connection";
import DevExpress from "devextreme";
import { IDataStoreServerActions } from "./data-store.server-actions";
import { IDataStoreClientActions } from "./data-store.client-actions";


/**
 * Represents WebSocket connection to the SignalR- hub that implements IDataSStoreServerActions
 * to provide CRUD operations for the Entity
 */
export abstract class DataStoreWsConnection<T> extends WsConnection implements IDataStoreServerActions<T>, IDataStoreClientActions<T> {
    constructor(
        protected readonly auth: AuthService,
        protected readonly activate$: Observable<boolean>
    ) {
        super(auth, activate$);
    }

    /**
     * Loaded result list of items
     * pushed after "load" called
     */
    public readonly loaded$ = new BehaviorSubject<T[]>([]);
    readonly loaded = (list: T[]) => this.loaded$.next(list);

    /**
     * Add new item to the DataStore from the server
     */
    public readonly added$ = new BehaviorSubject<T>(undefined);
    readonly added = (item: T) => this.added$.next(item);

    public override actions: SignalRAction[] = [
        { name: "loaded", handler: this.loaded },
        { name: "added", handler: this.added }
    ];
    
    public async load(loadOptions: DevExpress.data.LoadOptions): Promise<T[]> {
        await this.isConnectedPromise();
        await this.connection.invoke("load", loadOptions);
        return [];
        // return await firstValueFrom(this.loaded$);
    }

    public async insert(value: T) {
        await this.isConnectedPromise();
        await this.connection.invoke("insert", value);
        return value;
    }

    public async update(key, values) {
        await this.isConnectedPromise();
        await this.connection.invoke("update", key, values);
    }

    public async remove(key) {
        await this.isConnectedPromise();
        await this.connection.invoke("remove", key);
    }
}