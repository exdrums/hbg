import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { RestDataStore } from "@app/core/data/rest-data-store";
import { ConfigService } from "@app/core/services/config.service";
import { Distribution } from "devextreme/ui/box";

@Injectable()
export class DistributionsDataSource extends CustomDataSource<Distribution> {
    constructor(
        private readonly config: ConfigService,
        private readonly http: HttpClient
    ) {
        super(new RestDataStore<Distribution>(http, {
            key: "distributionID",
            loadUrl: `${config.hbgemailer}/api/distributions`,
            insertUrl: `${config.hbgemailer}/api/distributions/`,
            updateUrl: `${config.hbgemailer}/api/distributions/`,
            removeUrl: `${config.hbgemailer}/api/distributions/`,
            usePutForInsert: true
        }));
    }

    public readonly startDistribution = (distId: number) =>
        this.http.get(`${this.config.hbgemailer}/api/distributions/${distId}/start`).subscribe();
}
