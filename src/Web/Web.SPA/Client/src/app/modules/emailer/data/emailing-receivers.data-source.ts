import { HttpClient } from "@angular/common/http";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { RestDataStore } from "@app/core/data/rest-data-store";
import { ConfigService } from "@app/core/services/config.service";
import { EmailingReceiver } from "../models/emailing-receiver.model";

/** Not Injectable datasource becaulse of parameter  */
export class EmailingReceiversDataSource extends CustomDataSource<EmailingReceiver> {
    constructor(
        private readonly config: ConfigService,
        private readonly http: HttpClient,
        private readonly distributionID: number
    ) {
        super(new RestDataStore<EmailingReceiver>(http, {
            key: "receiverID",
            loadUrl: `${config.hbgemailer}/api/distributions/${distributionID}/emailingreceivers`,
            insertUrl: null,
            updateUrl: `${config.hbgemailer}/api/distributions/${distributionID}/emailingreceivers/`,
            removeUrl: null
        }));
    }
}
