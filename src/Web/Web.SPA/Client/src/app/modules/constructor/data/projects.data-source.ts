import { Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { ConfigService } from "@app/core/services/config.service";
import { HttpClient } from "@angular/common/http";
import { RestDataStore } from "@app/core/data/rest-data-store";
import { Project } from "../models";

@Injectable()
export class ProjectsDataSource extends CustomDataSource<Project, string> {
    constructor(private readonly config: ConfigService, private readonly http: HttpClient) {
        super(new RestDataStore<Project, string>(http, {
            key: "projectId",
            loadUrl: `${config.hbgconstructor}/api/projects`,
            insertUrl: `${config.hbgconstructor}/api/projects`,
            updateUrl: `${config.hbgconstructor}/api/projects/`,
            removeUrl: `${config.hbgconstructor}/api/projects/`
        }));
    }
}
