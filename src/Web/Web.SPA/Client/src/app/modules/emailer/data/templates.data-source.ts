import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CustomDataSource } from '@app/core/data/custom-data-source';
import { ConfigService } from '@app/core/services/config.service';
import { Template } from '../models/template.model';
import { RestDataStore } from '@app/core/data/rest-data-store';
import { firstValueFrom } from 'rxjs';

@Injectable()
export class TemplatesDataSource extends CustomDataSource<Template> {
    constructor(
        private readonly config: ConfigService,
        private readonly http: HttpClient
    ) {
        super(new RestDataStore<Template>(http, {
            key: "templateID",
            loadUrl: `${config.hbgemailer}/api/templates`,
            insertUrl: `${config.hbgemailer}/api/templates/`,
            updateUrl: `${config.hbgemailer}/api/templates/`,
            removeUrl: `${config.hbgemailer}/api/templates/`,
            usePutForInsert: true
        }));
        console.log('TemplatesDS_ctore', config);
    }
    /**
     * Returns full object of the template with Content
     * @param templateId PrimaryKey
     * @returns 
     */
    public readonly getFull$ = (templateId: number) => this.http.get<Template>(`${this.config.hbgemailer}/api/templates/${templateId}`);
}
