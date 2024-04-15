import { Injectable, inject } from "@angular/core";
import { AuthService } from "@app/core/services/auth.service";
import { ConfigService } from "@app/core/services/config.service";
import { SignalRAction, WsConnection } from "@app/core/services/websocket/ws-connection";

/**
 * SignalR Hub CRUD connection for Projects
 */
@Injectable()
export class ProjectsWebSocketConntection extends WsConnection {
  private config: ConfigService = inject(ConfigService);
  constructor(protected readonly auth: AuthService) {
    super(auth, auth.authStatus$)
  }
  public override actions: SignalRAction[] = [];
  protected hubUrl: string = `${this.config.hbgprojects}/hub/proj`
  protected canConnect: () => boolean = () => true;
}