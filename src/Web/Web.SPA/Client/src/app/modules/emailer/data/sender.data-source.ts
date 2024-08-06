import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { RestDataStore } from "@app/core/data/rest-data-store";
import { ConfigService } from "@app/core/services/config.service";
import { Sender } from "../models/sender.model";

@Injectable()
export class SendersDataSource extends CustomDataSource<Sender> {
    constructor(
        private readonly config: ConfigService,
        private readonly http: HttpClient
    ) {
        super(new RestDataStore<Sender>(http, {
            key: "senderID",
            loadUrl: `${config.hbgemailer}/api/senders`,
            insertUrl: `${config.hbgemailer}/api/senders/`,
            updateUrl: `${config.hbgemailer}/api/senders/`,
            removeUrl: `${config.hbgemailer}/api/senders/`,
            usePutForInsert: true
        }));
    }
}
