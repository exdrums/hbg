import { Injectable, inject } from "@angular/core";
import { AuthService } from "@app/core/services/auth.service";
import { DataStoreWsConnection } from "@app/core/services/websocket/data-store-ws-connection";
import { Project } from "../models/project.model";
import { ConfigService } from "@app/core/services/config.service";

/**
 * SignalR Hub CRUD connection for Projects
 */
@Injectable()
export class ProjectsWebSocketConntection extends DataStoreWsConnection<Project> {
  private config: ConfigService = inject(ConfigService);
  constructor(protected readonly auth: AuthService) {
    super(auth, auth.authStatus$)
  }
  protected hubUrl: string = `${this.config.hbgprojects}/hub/proj`
  protected canConnect: () => boolean = () => true;
}