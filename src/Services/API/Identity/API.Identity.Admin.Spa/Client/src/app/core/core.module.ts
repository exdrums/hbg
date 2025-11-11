import { NgModule, APP_INITIALIZER } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { OAuthModule } from 'angular-oauth2-oidc';

import { ConfigService } from './services/config.service';
import { AuthService } from './services/auth.service';
import { StorageService } from './services/storage.service';
import { ScreenService } from './services/screen.service';
import { AuthGuard } from './guards/auth.guard';
import { NoAuthGuard } from './guards/no-auth.guard';
import { DxLoadPanelModule, DxPopupModule, DxToolbarModule } from 'devextreme-angular';
import { PopupStackComponent } from './components/hbg-popup/popup-stack/popup-stack.component';
import { PopupContentComponent } from './components/hbg-popup/popup-content/popup-content.component';
import { MainHttpInterceptorProvider } from './errors/http.interceptor';
import { FrontEndErrorHandlerProvider } from './errors/error.handler';
import { PopupErrorComponent } from './components/hbg-popup/popup-error/popup-error.component';
import { ThemeService } from './services/theme.service';
import { NotificationService } from './services/notification.service';
import { PopupService } from './services/popup.service';
import { ScreenLockerService } from './services/screen-locker.service';

@NgModule({
  imports: [
    CommonModule,
    BrowserModule,
    HttpClientModule,
    OAuthModule.forRoot({
      resourceServer: {
        sendAccessToken: true,
        allowedUrls: []
      }
    }),
    DxPopupModule,
    DxLoadPanelModule,
    DxToolbarModule
  ],
  providers: [
    ConfigService,
    {
      provide: APP_INITIALIZER,
      useFactory: (config: ConfigService) => () => config.fetchAppSettings(),
      deps: [ConfigService],
      multi: true
    },
    AuthService,
    {
      provide: APP_INITIALIZER,
      useFactory: (config: ConfigService, auth: AuthService) => () => auth.connectOidc(),
      deps: [ConfigService, AuthService],
      multi: true
    },
    NotificationService,
    PopupService,
    ScreenLockerService,
    ScreenService,
    StorageService,
    ThemeService,

    AuthGuard,
    NoAuthGuard,

    MainHttpInterceptorProvider,
    FrontEndErrorHandlerProvider
  ],
  exports: [
    CommonModule,
    PopupStackComponent,
    DxLoadPanelModule
  ],
  declarations: [
    PopupStackComponent,
    PopupContentComponent,
    PopupErrorComponent
  ]
})
export class CoreModule { }
