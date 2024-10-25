import { inject, Injectable } from "@angular/core";
import { AuthService } from "@app/core/services/auth.service";
import { ConfigService } from "@app/core/services/config.service";
import { SignalRAction, WsConnection } from "@app/core/services/websocket/ws-connection";
import { filter, Subject, takeUntil, tap } from "rxjs";

@Injectable()
export class EmailerWebSocketConnection extends WsConnection implements IEmailerHubServerActions, IEmailerHubClientActions {
    private config: ConfigService = inject(ConfigService);
    constructor(protected readonly auth: AuthService) {
        super(auth, auth.authStatus$);
        void this.addHandlers();
    }
    public override actions: SignalRAction[] = [];
    protected hubUrl: string = `${this.config.hbgemailer}/hub`
    protected canConnect: () => boolean = () => true;

    private async addHandlers() {
        await this.isConnectedPromise();
        this.addHandler({ name: 'distributionUpdated', handler: x => this.distributionUpdated(x)});
    }

    public distributionUpdated$ = new Subject<DistributionUpdatedHubDto>();
    public distributionUpdated = (dto: DistributionUpdatedHubDto) => this.distributionUpdated$.next(dto);
    public subscribeDistributionUpdated$ = (distId: number) => this.distributionUpdated$.pipe(
        // tap(x => console.log('distributionUpdated$ ', x)),
        // takeUntil(this.isDisconnected$),
        filter(dto => dto.distributionId === distId)
    );

    public async trackDistributions(distIds: number[]) {
        await this.isConnectedPromise();
        this.connection.invoke("trackDistributions", distIds);
    }

    public async trackDistribution(distId: number) {
        await this.isConnectedPromise();
        this.connection.invoke("TrackDistribution", distId);
    }
}

export interface IEmailerHubClientActions {
    distributionUpdated: (dto: DistributionUpdatedHubDto) => void;
}

export interface IEmailerHubServerActions {
    trackDistributions: (distIds: number[]) => void;
    trackDistribution: (distId: number) => void;
}

export interface DistributionUpdatedHubDto {
    distributionId: number;
    emailsSent: number;
    emailsPending: number;
    emailsError: number;
}