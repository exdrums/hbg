import { Injectable } from "@angular/core";
import { CustomDataSource } from "../custom-data-source";
import { Message } from "@app/core/models/contacts/message.model";
import { HttpClient } from "@angular/common/http";
import { ConfigService } from "@app/core/services/config.service";
import { RestDataStore } from "../rest-data-store";

@Injectable({ providedIn: 'root' })
export class MessagesDataSource extends CustomDataSource<Message> { 
    constructor(private readonly config: ConfigService, private readonly http: HttpClient) {
        super(new RestDataStore<Message, number>(http, {
            key: "messageID",
            // TODO: fix it
            // loadUrl: `${this.config.appSetings.hbgcontacts}/api/contacts/${contactID}/messages`,
            // insertUrl: `${this.config.appSetings.hbgcontacts}/api/contacts/${contactID}/messages`
        }));
    }
}