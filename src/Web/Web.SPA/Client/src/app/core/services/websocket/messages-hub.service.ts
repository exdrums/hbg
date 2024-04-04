import { Injectable } from "@angular/core";
import { Message_HUB } from "@app/core/models/contacts/message.model";
import { environment } from "@env/environment";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { MessagePackHubProtocol } from "@microsoft/signalr-protocol-msgpack/dist/esm/MessagePackHubProtocol";
import { BehaviorSubject } from "rxjs";
import { AuthService } from "../auth.service";
import { ConfigService } from "../config.service";
import { NotificationService } from "../notification.service";

@Injectable({
  providedIn: "root",
})
export class MessagesHubService {
  /** Reference to the current HubConnection */
  private hubConnection: HubConnection;
  public connected$ = new BehaviorSubject<boolean>(false);
  private logDebug = environment.production ? false : true; // activate here

  constructor(
    private config: ConfigService, 
    private auth: AuthService,
    private notify: NotificationService
  ) {
    // this.auth.authStatus$.subscribe((s) => {
    //   if (s) this.connect();
    //   else if (this.hubConnection) this.disconnect();
    // });
  }

  private connect() {
    this.hubConnection = this.getConnection(
      `${this.config.hbgprojects}/api/hubs/messages`
    );
    this.startConnection();
    this.addListeners();
  }

  private disconnect() {
    this.hubConnection.stop().then(() => {
      this.hubConnection = null;
      console.warn("Hub connection stoped");
    });
  }

  /** Create HubConnection */
  private getConnection(connectionUrl: string): HubConnection {
    return new HubConnectionBuilder()
      .withUrl(connectionUrl, {
        accessTokenFactory: () => this.auth.getToken(),
      })
      .withHubProtocol(new MessagePackHubProtocol())
      .configureLogging(this.logDebug ? LogLevel.Debug : LogLevel.Error)
      .withAutomaticReconnect()
      .build();
  }

  private startConnection(): void {
    this.hubConnection
      .start()
      .then(() => {
        this.connected$.next(true);
      })
      .catch((err) => {
        console.error("Error while establishing connection...");
      });
  }

  private addListeners() {
    this.hubConnection.on("NewMessage", (data: Message_HUB) => {
      console.log("NewMessage", data);
      this.notify.newMessage$.next(data.text);
    });
    this.hubConnection.on("SystemMessage", (data) => {
      console.log("SystemMessage", data);
    });
  }
}

