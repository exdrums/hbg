import { Injectable } from "@angular/core";
import { ConfigService } from "./config.service";
import { AuthConfig, OAuthService, OAuthModuleConfig } from "angular-oauth2-oidc";
import { BehaviorSubject } from "rxjs";
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

      // The SPA's id. The SPA is registerd with this id at the auth-server
      clientId: "js",

      // Just needed if your auth server demands a secret. In general, this
      // is a sign that the auth server is not configured with SPAs in mind
      // and it might not enforce further best practices vital for security
      // such applications.
      dummyClientSecret: "js_secret",
    
      scope: "openid profile email roles offline_access api_files api_projects api_emailer",
      showDebugInformation: true,
      oidc: false,
      useSilentRefresh: true
    };

    this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgidentity);
    this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgfiles);
    this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgprojects);
    this.moduleConfig.resourceServer.allowedUrls.push(this.configs.hbgemailer);


    this.auth.setStorage(sessionStorage);

    this.auth.configure(conf);

    const x = await this.auth.loadDiscoveryDocument(); // .then(x => {
    this.auth.setupAutomaticSilentRefresh();
    if (this.auth.hasValidAccessToken()) {
      const y = await this.auth.loadUserProfile(); //then(y => {
      if (y) {
        // console.log('UserLoaded', y);
        this.loginInfo = x;
        this.authStatus$.next(true);
      }
      this.authStatus$.next(true);
      // console.log("CompleteAuthSetup");
      // console.log("this.authStatus", this.authStatus);
    } else {
      // console.log("Has no token");
      this.authStatus$.next(false);
    }
  }

  public authStatus$ = new BehaviorSubject<boolean>(true); 
  public get authStatus(): boolean {
    return this.authStatus$.value;
  }
  private loginInfo: any;

  
  public login(lgn: string, psw: string) {
    // console.log("Loginn...");
    this.auth
      .fetchTokenUsingPasswordFlowAndLoadUserProfile(lgn, psw)
      .catch(e => this.notification.newMessage$.next(e))
      .then((x) => {
        this.loginInfo = x;
        this.authStatus$.next(true);
        this.router.navigate(["/home"]);
      });
  }

  public readonly getToken = () => this.auth.getAccessToken();

  public changePassword(psw: string, code: string) {
    this.notification.newMessage$.next("Change password not implemented");
  }

  public createAccount(email: string, psw: string) {
    this.notification.newMessage$.next("Create account not implemented");
  }
  
  public resetPassword(email: string) {
    this.notification.newMessage$.next("Reset password not implemented");
  }

  public logout() {
    this.authStatus$.next(false);
    this.auth.logOut();
  }

  // public initLogin(s: IAppSettings) {
  //   // Create main auth configuration
  //   const conf: AuthConfig = {
  //     // Url of the Identity Provider
  //     issuer: s.hbgidentity,

  //     // URL of the SPA to redirect the user to after login
  //     // redirectUri: window.location.origin + '/index.html',
  //     redirectUri: window.location.origin + "/",
  //     responseType: "code",

  //     // The SPA's id. The SPA is registerd with this id at the auth-server
  //     clientId: "js",
  //     // Just needed if your auth server demands a secret. In general, this
  //     // is a sign that the auth server is not configured with SPAs in mind
  //     // and it might not enforce further best practices vital for security
  //     // such applications.

  //     dummyClientSecret: "js_secret",

  //     scope: "openid profile email roles api_files api_catalog api_contacts",
  //     showDebugInformation: true,
  //   };
  //   // allowed urls for AccessToken
  //   this.moduleConfig.resourceServer.allowedUrls.push(s.hbgcatalog);

  //   this.auth.configure(conf);
  //   // this.auth.tokenValidationHandler = new JwksValidationHandler();
  //   this.auth
  //     .loadDiscoveryDocumentAndLogin()
  //     .then((x) => this.authStatus$.next(x));
  // }

  // password flow

  // private initPasswordFlow(s: IAppSettings) {
  //   // Create main auth configuration
  //   const conf: AuthConfig = {
  //     // Url of the Identity Provider
  //     issuer: s.hbgidentity,

  //     // URL of the SPA to redirect the user to after login
  //     // redirectUri: window.location.origin + '/index.html',
  //     redirectUri: window.location.origin + "/",
  //     // responseType: ,

  //     // The SPA's id. The SPA is registerd with this id at the auth-server
  //     clientId: "js",
  //     // Just needed if your auth server demands a secret. In general, this
  //     // is a sign that the auth server is not configured with SPAs in mind
  //     // and it might not enforce further best practices vital for security
  //     // such applications.

  //     dummyClientSecret: "js_secret",

  //     scope: "openid profile email roles api_files api_catalog api_contacts",
  //     showDebugInformation: true,
  //     oidc: false,
  //   };
  //   // allowed urls for AccessToken
  //   this.moduleConfig.resourceServer.allowedUrls.push(s.hbgcatalog);
  //   this.auth.setStorage(sessionStorage);

  //   this.auth.configure(conf);
  //   // this.auth.tokenValidationHandler = new JwksValidationHandler();
  //   // this.auth.loadDiscoveryDocument();
  //   this.auth
  //     .loadDiscoveryDocument()
  //     .then((x) => {
  //       // console.log('Success??', x);
  //       // console.log('this.auth.state', this.auth.state);
  //       if (this.auth.hasValidAccessToken()) {
  //         this.auth
  //           .loadUserProfile()
  //           .then((y) => {
  //             if (y) {
  //               // console.log('UserLoaded', y);
  //               this.loginInfo = x;
  //               this.authStatus$.next(true);
  //             }
  //             // console.log('LoadUserProf', y);
  //             // console.log('this.auth.state', this.auth.state);
  //             this.authStatus$.next(true);
  //             console.log("CompleteAuthSetup");
  //           })
  //           .catch((x) => console.error("OIDC: LoadUserProfileError", x));
  //       }
  //       // this.authStatus$.next(x)
  //     })
  //     .catch((x) => console.error("OIDC: LoadDiscoveryDocError", x));
  // }

}
