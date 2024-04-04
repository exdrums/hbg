import { Injectable } from "@angular/core";
import { CustomDataSource } from "../custom-data-source";
import { Contact } from "@app/core/models/contacts/contract.model";
import { ConfigService } from "@app/core/services/config.service";
import { HttpClient } from "@angular/common/http";
import { RestDataStore } from "../rest-data-store";

@Injectable()
export class ContactsDataSource extends CustomDataSource<Contact> { 
    constructor(private readonly config: ConfigService, private readonly http: HttpClient) {
        super(new RestDataStore<Contact, number>(http, {
            key: "contactId",
            loadUrl: `${config.hbgprojects}/api/contacts`
        }));
    }
}