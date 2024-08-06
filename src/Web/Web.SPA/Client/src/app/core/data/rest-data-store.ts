import { HttpClient, HttpParams } from "@angular/common/http";
import { isNotEmpty } from "@app/core/utils/string-utils";
import DevExpress from "devextreme";
import { firstValueFrom, of } from "rxjs";
import { DataStoreError } from "./errors";
import { LoadedData, RestDataStoreUrls } from "./options";
import { CustomDataStore } from "./custom-data-store";

/**
 * See functions of AspNetData.createStore if you hav troubles with requests construction
 */
export class RestDataStore<TGET, TKEY = number, TPOST = TGET> extends CustomDataStore<TGET, TKEY> {
    constructor(private readonly http: HttpClient, protected urls: RestDataStoreUrls) {
        super({
            key: urls.key,
            load: (loadOpts) => this.loadAction(loadOpts, urls.loadUrl),
            insert: (values) => this.insertAction(values, urls.insertUrl),
            update: (key, values) => this.updateAction(key, values, urls.updateUrl),
            remove: (key) => this.removeAction(key, urls.removeUrl)
        });
        // console.log('RestDataStore_ctor', this, urls);
    }

    //#region Standardized requests

    private loadAction(loadOptions: DevExpress.data.LoadOptions, url: string, extraArgament?: { key: string, value: string }[]) {
        // console.log('loadAction.LoadOptions', loadOptions);
        if (!url) console.error("Load-URL is null. Cannot call loadAction without URL.");
    
        let params: HttpParams = new HttpParams();
        ["skip", "take", "requireTotalCount", "requireGroupCount", "sort", "filter", "totalSummary", "group", "groupSummary"].forEach(function (i) {
            if (i in loadOptions && isNotEmpty(loadOptions[i]))
                params = params.set(i, JSON.stringify(loadOptions[i]));
        });
        if (extraArgament !== undefined) {
            extraArgament.forEach(arg => {
                params = params.set(arg.key, arg.value);
            });
        }
        return firstValueFrom(this.http.get<LoadedData<TGET[]>>(url, { params: params }).pipe(
            // tap(x => console.log('LoadDxResult', x)),
            // catchError(() => { console.log('HTTP Catch error'); return of(null);})
            // map(response => {
            //     return {
            //         data: response.data,
            //         totalCount: response.totalCount,
            //         summary: response.summary,
            //         groupCount: response.groupCount
            //     };
            // })
        ))
    }
    
    /**
     * Create a new item of the TPOST type with POST action to the API
     * @param values TODO: check object and set type here
     * @param url 
     * @param extgraArgament 
     * @returns 
     */
    private insertAction( values: any, url: string, extgraArgament?: {key: string, value: string}[]) {
        if(!url) {
            console.error("Insert-URL is null. Cannot call insertAction without URL.");
            return firstValueFrom(of(null));
        }
        let params: HttpParams = new HttpParams();
        if (extgraArgament !== undefined) {
            extgraArgament.forEach(arg => {
                params = params.set(arg.key, arg.value);
            });
        }
        return firstValueFrom(this.http.post<any>(url, values, { params: params }));
    }

    private updateAction(key: any, values: any, url: string) {
        if(!url) throw new DataStoreError("Update-URL is null. Cannot call updateAction without URL.");
        return firstValueFrom(this.http.put(url + key, values));
    }

    private removeAction(key: any, url: string) {
        if(!url) throw new DataStoreError("Remove-URL is null. Cannot call removeAction without URL.");
        return firstValueFrom(this.http.delete<void>(url + key));
    }

    // function insertManyAction(values: any, url: string,) {
    //     return this.http.post(url, values);
    // }
    // function deleteManyAction(keys: number[], url: string) {
    //     return this.http.request('DELETE', url, { body: keys });
    // }

    //#endregion
}