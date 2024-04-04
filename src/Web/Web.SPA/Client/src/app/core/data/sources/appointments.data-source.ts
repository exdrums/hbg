import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import { Appointment } from '@app/modules/scheduler/models/appointment';
import { CustomDataSource } from '../custom-data-source';
import { RestDataStore } from '../rest-data-store';

@Injectable()
export class AppointmentsDataSource extends CustomDataSource<Appointment> {
    constructor(
        private readonly config: ConfigService,
        private readonly http: HttpClient
    ) {
        super(new RestDataStore<Appointment>(http, {
            key: "id",
            loadUrl: `${config.hbgprojects}/api/appointments/`,
            insertUrl: `${config.hbgprojects}/api/appointments/`,
            updateUrl: `${config.hbgprojects}/api/appointments/`,
            removeUrl: `${config.hbgprojects}/api/appointments/`
        }));
    }
}
