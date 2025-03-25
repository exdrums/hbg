import { HttpTransportType, HubConnection, HubConnectionBuilder, HubConnectionState, IHubProtocol, JsonHubProtocol, LogLevel } from "@microsoft/signalr";
import { BehaviorSubject, Observable, delay, filter, firstValueFrom, map, take, tap } from "rxjs";
import { AuthService } from "../auth.service";
import { environment } from "@env/environment";

export interface SignalRAction<T = any> {
	name: string,
	handler: (message: T) => void
}

export abstract class WsConnection {
	constructor(
		protected readonly auth: AuthService,
		protected readonly activate$: Observable<boolean>,
		protected readonly protocol: IHubProtocol = new JsonHubProtocol()
	) {
		this.activation$().subscribe();
	}
	
	private logDebug = environment.production ? false : true; // activate here
	protected abstract hubUrl: string;
	protected abstract canConnect(): boolean;
	protected abstract actions: SignalRAction[];
	public connection: HubConnection;
	public isConnected$ = new BehaviorSubject<boolean>(false);
	public isConnectedPromise = () => firstValueFrom(this.isConnected$.pipe(filter(o => o)));
	public isConnected = () => this.connection != null && this.connection.state === HubConnectionState.Connected;
	public isDisconnected$ = this.isConnected$.pipe(filter(o => o === false));

	/** Add new handler function to the connection */
	public readonly addHandler = (action: SignalRAction) => this.connection.on(action.name, action.handler);
	public readonly invoke = (name: string, ...args: any[]) => this.connection.invoke(name, ...args);
	/**
	 * Activate: true => connect WebSocket
	 * Activate: false => disconnect WebSocket
	 */
	private activation$ = () => this.activate$.pipe().pipe(
		delay(0),
		map((status) => {
			if (status) {
				// Check if tracking is allowed and connect if possible
				this.checkConnectionAllowedAndConnect();
			}
			else {
				void this.disconnect();
			}
		}),
	);

	/**
	 * Check permissions for this WS and connect
	 */
	private checkConnectionAllowedAndConnect() {
		if ((!this.canConnect) || this.canConnect()) {
			void this.connect();
		}
	}

	/**
	 * Connect to SignalR Hub
	 */
	private readonly connect = async () => {
		this.connection = this.createConnection();
		this.registerHandlers(this.connection);

		await this.connection.start().catch(x => console.error('Starting WebSocket failed', x));
		this.isConnected$.next(this.isConnected());
	}

	private readonly createConnection = () => new HubConnectionBuilder()
		.withUrl(this.hubUrl, {
			accessTokenFactory: () => this.auth.getToken(),
			transport: HttpTransportType.WebSockets
		})
		.withAutomaticReconnect()
		.configureLogging(this.logDebug ? LogLevel.Debug : LogLevel.Error)
		.withHubProtocol(this.protocol)
		.build();

	private registerHandlers(connection: HubConnection) {
		// Register default lifecycle handlers
		connection.onreconnecting(this.onreconnecting);
		connection.onreconnected(this.onreconnected);
		connection.onclose(this.onclose);

		// register custom handlers
		for (const action of this.actions) {
			connection.on(action.name, action.handler);
		}
	}

	private readonly onclose = (error: Error) => {
		// if Logout => WS closed => set status to undefined to reset availability state
		this.activate$.pipe(
			take(1),
			tap(x => this.isConnected$.next(x === false ? undefined : true))
		).subscribe();

		if (error != null)
			console.error('!!!WebSocket Closed', error);
	}

	private readonly onreconnecting = (error: Error) => {
		this.isConnected$.next(false);
		console.warn('!!!WebSocket Reconnecting', error);
	}

	private readonly onreconnected = (connectionId: string) => {
		this.isConnected$.next(true);
		console.warn('!!!WebSocket Reconnected', connectionId);
	}

	/**
	 * Disconnect SignalR Hub
	 */
	private readonly disconnect = () => {
		console.info('WebSocket => Disconnected');
		this.isConnected$.next(false);
		void this.connection?.stop();
	}
}