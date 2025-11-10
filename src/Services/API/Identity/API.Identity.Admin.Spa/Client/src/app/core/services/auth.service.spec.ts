import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from './auth.service';
import { ConfigService } from './config.service';
import { NotificationService } from './notification.service';
import { of, Subject } from 'rxjs';

describe('AuthService', () => {
  let service: AuthService;
  let mockOAuthService: jasmine.SpyObj<OAuthService>;
  let mockConfigService: jasmine.SpyObj<ConfigService>;
  let mockNotificationService: jasmine.SpyObj<NotificationService>;

  beforeEach(() => {
    mockOAuthService = jasmine.createSpyObj('OAuthService', [
      'configure',
      'loadDiscoveryDocument',
      'loadUserProfile',
      'setupAutomaticSilentRefresh',
      'fetchTokenUsingPasswordFlowAndLoadUserProfile',
      'logOut',
      'hasValidAccessToken',
      'getAccessToken',
      'getIdentityClaims',
      'setStorage'
    ]);

    mockConfigService = jasmine.createSpyObj('ConfigService', ['isReady'], {
      ready$: of(true),
      hbgidentity: 'https://test-sts.local',
      hbgidentityadminspa: 'https://test-admin-spa.local',
      hbgidentityadminapi: 'https://test-admin-api.local'
    });
    mockConfigService.isReady.and.returnValue(Promise.resolve(true));

    mockNotificationService = jasmine.createSpyObj('NotificationService', [], {
      newMessage$: new Subject()
    });

    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [
        AuthService,
        { provide: OAuthService, useValue: mockOAuthService },
        { provide: ConfigService, useValue: mockConfigService },
        { provide: NotificationService, useValue: mockNotificationService },
        {
          provide: 'OAuthModuleConfig',
          useValue: {
            resourceServer: {
              allowedUrls: [],
              sendAccessToken: false
            }
          }
        }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should configure OAuth on connectOidc', async () => {
    mockOAuthService.loadDiscoveryDocument.and.returnValue(Promise.resolve({ type: 'discovery_document_loaded', info: {} } as any));
    mockOAuthService.hasValidAccessToken.and.returnValue(false);

    await service.connectOidc();

    expect(mockOAuthService.configure).toHaveBeenCalled();
    expect(mockOAuthService.loadDiscoveryDocument).toHaveBeenCalled();
  });

  it('should setup automatic silent refresh', async () => {
    mockOAuthService.loadDiscoveryDocument.and.returnValue(Promise.resolve({ type: 'discovery_document_loaded', info: {} } as any));
    mockOAuthService.hasValidAccessToken.and.returnValue(false);

    await service.connectOidc();

    expect(mockOAuthService.setupAutomaticSilentRefresh).toHaveBeenCalled();
  });

  it('should login with username and password', () => {
    const username = 'testuser';
    const password = 'testpass';
    const mockUserInfo = { sub: 'user123', name: 'Test User' };
    mockOAuthService.fetchTokenUsingPasswordFlowAndLoadUserProfile.and.returnValue(Promise.resolve(mockUserInfo));
    mockOAuthService.getIdentityClaims.and.returnValue(mockUserInfo);

    service.login(username, password);

    expect(mockOAuthService.fetchTokenUsingPasswordFlowAndLoadUserProfile).toHaveBeenCalledWith(username, password);
  });

  it('should logout and redirect', () => {
    service.logout();
    expect(mockOAuthService.logOut).toHaveBeenCalled();
  });

  it('should check if user is authenticated', () => {
    mockOAuthService.hasValidAccessToken.and.returnValue(true);
    expect(service.isAuthenticated()).toBeTrue();

    mockOAuthService.hasValidAccessToken.and.returnValue(false);
    expect(service.isAuthenticated()).toBeFalse();
  });

  it('should get access token', () => {
    const testToken = 'test-access-token';
    mockOAuthService.getAccessToken.and.returnValue(testToken);

    expect(service.getToken()).toBe(testToken);
  });

  it('should provide user profile observable', (done) => {
    service.userProfile$.subscribe(profile => {
      // Initially null
      expect(profile).toBeNull();
      done();
    });
  });
});
