import DevExpress from "devextreme";
import { DeepPartial } from "devextreme/core";
import { WsConnection } from "../services/websocket/ws-connection";
import { dxTimeout } from "../utils/dx-utils";
import { CustomDataStore } from "./custom-data-store";


export class SignalRDataStore<TGET, TKEY = number, TPOST = TGET> extends CustomDataStore<TGET, TKEY>  {
    constructor(
        private connection: WsConnection,
        private keyProp: "projectID" | "planID" | "articleID",
        private entity: "Project" | "Plan" | "Article"
    ) {
        super({
            key: keyProp,
            loadMode: "raw",
            cacheRawData: true,
            load: (loadOptions: DevExpress.data.LoadOptions) => this.loadAction(loadOptions),
            insert: (value: TGET) => this.insertAction(value),
            update: (key, values) => this.updateAction(key, values),
            remove: (key) => this.removeAction(key),
            onPush: (changes) => console.log('onPush_changes', changes)
        });
        void this.addHandlers();
    }


    //#region Handlers

    private async addHandlers() {
        await this.connection.isConnectedPromise();
        this.connection.addHandler({ name: "loaded" + this.entity, handler: this.loaded });
        this.connection.addHandler({ name: "added" + this.entity, handler: this.added });
    }

    private readonly loaded = async (items: any[]) => {
        await dxTimeout();
        this.push(items.map(i => ({ type: "insert", data: i as DeepPartial<TGET>, index: 0 })));
    }

    private readonly added = async (item: TGET) => {
        await dxTimeout();
        console.log('Added', item);
        this.push([{ type: "insert", data: item as DeepPartial<TGET> }]);
    }

    //#endregion

    //#region Actions

    public async loadAction(loadOptions: DevExpress.data.LoadOptions): Promise<TGET[]> {
        await this.connection.isConnectedPromise();
        var result = await this.connection.connection.invoke("load" + this.entity, loadOptions, this.subjectId);
        return result.data;
    }

    public async insertAction(value: TGET): Promise<TGET> {
        await this.connection.isConnectedPromise();
        const result: TGET = await this.connection.connection.invoke("insert" + this.entity, value, this.subjectId);
        return result;
    }

    public async updateAction(key, values) {
        await this.connection.isConnectedPromise();
        await this.connection.connection.invoke("update" + this.entity, key, values, this.subjectId);
    }

    public async removeAction(key) {
        await this.connection.isConnectedPromise();
        await this.connection.connection.invoke("remove" + this.entity, key, this.subjectId);
    }
    //#endregion
    
}