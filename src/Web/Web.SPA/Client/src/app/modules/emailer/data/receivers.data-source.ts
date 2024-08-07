import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { RestDataStore } from "@app/core/data/rest-data-store";
import { ConfigService } from "@app/core/services/config.service";
import { Receiver } from "../models/receiver.model";

@Injectable()
export class ReceiversDataSource extends CustomDataSource<Receiver> {
    constructor(
        private readonly config: ConfigService,
        private readonly http: HttpClient
    ) {
        super(new RestDataStore<Receiver>(http, {
            key: "receiverID",
            loadUrl: `${config.hbgemailer}/api/receivers`,
            insertUrl: `${config.hbgemailer}/api/receivers/`,
            updateUrl: `${config.hbgemailer}/api/receivers/`,
            removeUrl: `${config.hbgemailer}/api/receivers/`,
            usePutForInsert: true
        }));
    }
}
