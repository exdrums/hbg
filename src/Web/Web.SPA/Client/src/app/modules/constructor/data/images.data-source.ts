import { Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { ConfigService } from "@app/core/services/config.service";
import { HttpClient } from "@angular/common/http";
import { RestDataStore } from "@app/core/data/rest-data-store";
import { GeneratedImage } from "../models";

@Injectable()
export class ImagesDataSource extends CustomDataSource<GeneratedImage> {
    private projectId: string | null = null;

    constructor(private readonly config: ConfigService, private readonly http: HttpClient) {
        super(new RestDataStore<GeneratedImage, string>(http, {
            key: "imageId",
            loadUrl: `${config.hbgconstructor}/api/projects/placeholder/images`,
            removeUrl: `${config.hbgconstructor}/api/images/`
        }));
    }

    setProjectId(projectId: string) {
        this.projectId = projectId;
        // Update the load URL dynamically
        const store = this.store() as RestDataStore<GeneratedImage, string>;
        (store as any).urls.loadUrl = `${this.config.hbgconstructor}/api/projects/${projectId}/images`;
        this.reload();
    }
}
