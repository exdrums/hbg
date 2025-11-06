import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';

import { ClientFormComponent } from './client-form.component';
import { ClientsService } from '../services/clients.service';
import { ConfigService } from '@app/core/services/config.service';
import { SharedModule } from '@app/shared/shared.module';

describe('ClientFormComponent', () => {
  let component: ClientFormComponent;
  let fixture: ComponentFixture<ClientFormComponent>;
  let mockClientsService: jasmine.SpyObj<ClientsService>;
  let mockConfigService: jasmine.SpyObj<ConfigService>;

  beforeEach(async () => {
    mockClientsService = jasmine.createSpyObj('ClientsService', [], {
      dataSource: jasmine.createSpyObj('CustomDataSource', ['store', 'insert', 'update'])
    });

    mockConfigService = jasmine.createSpyObj('ConfigService', [], {
      hbgidentityadminapi: 'https://test-api.local'
    });

    await TestBed.configureTestingModule({
      declarations: [ClientFormComponent],
      imports: [
        SharedModule,
        RouterTestingModule,
        HttpClientTestingModule
      ],
      providers: [
        { provide: ClientsService, useValue: mockClientsService },
        { provide: ConfigService, useValue: mockConfigService },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { paramMap: { get: () => null } }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ClientFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize in create mode when no id parameter', () => {
    fixture.detectChanges();
    expect(component.isEditMode).toBeFalse();
    expect(component.clientId).toBeNull();
  });

  it('should initialize with empty client object', () => {
    expect(component.client).toBeDefined();
    expect(component.client.clientId).toBe('');
    expect(component.client.clientName).toBe('');
    expect(component.client.enabled).toBeTrue();
  });

  it('should have protocol types defined', () => {
    expect(component.protocolTypes).toEqual(['oidc', 'saml2p', 'wsfed']);
  });

  it('should have grant types defined', () => {
    expect(component.availableGrantTypes).toContain('authorization_code');
    expect(component.availableGrantTypes).toContain('client_credentials');
    expect(component.availableGrantTypes).toContain('password');
  });

  it('should have token usage options', () => {
    expect(component.refreshTokenUsageEditorOptions.items.length).toBe(2);
    expect(component.refreshTokenUsageEditorOptions.displayExpr).toBe('text');
    expect(component.refreshTokenUsageEditorOptions.valueExpr).toBe('value');
  });

  it('should add redirect URI to list', () => {
    const uri = 'https://test.local/callback';
    component.onAddRedirectUri(uri);
    expect(component.redirectUris).toContain(uri);
  });

  it('should not add duplicate redirect URI', () => {
    const uri = 'https://test.local/callback';
    component.onAddRedirectUri(uri);
    component.onAddRedirectUri(uri);
    expect(component.redirectUris.length).toBe(1);
  });

  it('should remove redirect URI from list', () => {
    const uri = 'https://test.local/callback';
    component.redirectUris = [uri];
    component.onRemoveRedirectUri(uri);
    expect(component.redirectUris).not.toContain(uri);
  });

  it('should validate required fields before save', async () => {
    component.client.clientId = '';
    component.client.clientName = '';
    await component.onSave();
    expect(component.isSaving).toBeFalse();
  });
});
