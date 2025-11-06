import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from './auth.service';
import { ConfigService } from './config.service';
import { of } from 'rxjs';

describe('AuthService', () => {
  let service: AuthService;
  let mockOAuthService: jasmine.SpyObj<OAuthService>;
  let mockConfigService: jasmine.SpyObj<ConfigService>;

  beforeEach(() => {
    mockOAuthService = jasmine.createSpyObj('OAuthService', [
      'configure',
      'loadDiscoveryDocumentAndTryLogin',
      'setupAutomaticSilentRefresh',
      'fetchTokenUsingPasswordFlow',
      'logOut',
      'hasValidAccessToken',
      'getAccessToken',
      'getIdentityClaims'
    ]);

    mockConfigService = jasmine.createSpyObj('ConfigService', [], {
      ready$: of(true),
      hbgidentity: 'https://test-sts.local',
      hbgidentityadminspa: 'https://test-admin-spa.local'
    });

    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [
        AuthService,
        { provide: OAuthService, useValue: mockOAuthService },
        { provide: ConfigService, useValue: mockConfigService }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should configure OAuth on connectOidc', async () => {
    mockOAuthService.loadDiscoveryDocumentAndTryLogin.and.returnValue(Promise.resolve(true));

    await service.connectOidc();

    expect(mockOAuthService.configure).toHaveBeenCalled();
    expect(mockOAuthService.loadDiscoveryDocumentAndTryLogin).toHaveBeenCalled();
  });

  it('should setup automatic silent refresh', async () => {
    mockOAuthService.loadDiscoveryDocumentAndTryLogin.and.returnValue(Promise.resolve(true));

    await service.connectOidc();

    expect(mockOAuthService.setupAutomaticSilentRefresh).toHaveBeenCalled();
  });

  it('should login with username and password', () => {
    const username = 'testuser';
    const password = 'testpass';
    mockOAuthService.fetchTokenUsingPasswordFlow.and.returnValue(Promise.resolve());

    service.login(username, password);

    expect(mockOAuthService.fetchTokenUsingPasswordFlow).toHaveBeenCalledWith(username, password);
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

    expect(service.getAccessToken()).toBe(testToken);
  });

  it('should provide user profile observable', (done) => {
    service.userProfile$.subscribe(profile => {
      // Initially null
      expect(profile).toBeNull();
      done();
    });
  });
});
