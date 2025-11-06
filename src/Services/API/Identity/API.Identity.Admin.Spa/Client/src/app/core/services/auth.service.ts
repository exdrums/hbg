import { Injectable } from "@angular/core";
import { ConfigService } from "./config.service";
import { AuthConfig, OAuthService, OAuthModuleConfig } from "angular-oauth2-oidc";
import { BehaviorSubject, Observable } from "rxjs";
import { Router } from "@angular/router";
import { NotificationService } from "./notification.service";

@Injectable()
export class AuthService {
  constructor(
    private configs: ConfigService,
    private auth: OAuthService,
    private moduleConfig: OAuthModuleConfig,
    private readonly router: Router,
    private notification: NotificationService
  ) {
  }

  /** Init Auth protocol with APP_INITIALIZER */
  async connectOidc(): Promise<void> {
    const configReady = await this.configs.isReady();

    const conf: AuthConfig = {
      // Url of the Identity Provider
      issuer: this.configs.hbgidentity,

      // URL of the SPA to redirect the user to after login
      redirectUri: window.location.origin + "/",

      // The SPA's id - client for Identity Admin SPA
      clientId: "client_admin_spa",

      // Client secret for admin spa
      dummyClientSecret: "admin_spa_secret",

      scope: "openid profile email roles offline_access api_admin",
      showDebugInformation: true,
      oidc: false,
      useSilentRefresh: true,
      requireHttps: false
    };

    this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgidentity);
    this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgidentityadminapi);
    this.moduleConfig.resourceServer.sendAccessToken = true;

    this.auth.setStorage(sessionStorage);
    this.auth.configure(conf);

    const x = await this.auth.loadDiscoveryDocument();
    this.auth.setupAutomaticSilentRefresh();

    if (this.auth.hasValidAccessToken()) {
      const y = await this.auth.loadUserProfile();
      if (y) {
        this.loginInfo = x;
        this.authStatus$.next(true);
        this.userProfileSubject.next(this.auth.getIdentityClaims());
      }
      this.authStatus$.next(true);
    } else {
      this.authStatus$.next(false);
    }
  }

  public authStatus$ = new BehaviorSubject<boolean>(true);

  public get isAuthenticated$(): Observable<boolean> {
    return this.authStatus$;
  }

  private userProfileSubject = new BehaviorSubject<any>(null);
  public get userProfile$(): Observable<any> {
    return this.userProfileSubject.asObservable();
  }

  public get authStatus(): boolean {
    return this.authStatus$.value;
  }

  public isAuthenticated(): boolean {
    return this.authStatus$.value;
  }

  private loginInfo: any;

  public login(lgn: string, psw: string) {
    this.auth
      .fetchTokenUsingPasswordFlowAndLoadUserProfile(lgn, psw)
      .catch(e => this.notification.newMessage$.next(e))
      .then((x) => {
        this.loginInfo = x;
        this.authStatus$.next(true);
        this.userProfileSubject.next(this.auth.getIdentityClaims());
        this.router.navigate(["/dashboard"]);
      });
  }

  public readonly getToken = () => {
    return this.auth.getAccessToken();
  }

  public logout() {
    this.authStatus$.next(false);
    this.userProfileSubject.next(null);
    this.auth.logOut();
  }
}
