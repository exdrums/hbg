import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ClientsService } from '../services/clients.service';
import { Client } from '../models/client.model';
import notify from 'devextreme/ui/notify';
import { confirm } from 'devextreme/ui/dialog';

@Component({
  selector: 'hbg-client-form',
  templateUrl: './client-form.component.html',
  styleUrls: ['./client-form.component.scss'],
  providers: [ClientsService]
})
export class ClientFormComponent implements OnInit {
  clientId: string | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;

  client: Client = this.getEmptyClient();

  // Available options for dropdowns
  protocolTypes = ['oidc', 'saml2p', 'wsfed'];

  availableGrantTypes = [
    'authorization_code',
    'client_credentials',
    'password',
    'implicit',
    'hybrid',
    'urn:ietf:params:oauth:grant-type:device_code',
    'refresh_token'
  ];

  refreshTokenUsageOptions = [
    { value: 0, text: 'ReUse' },
    { value: 1, text: 'OneTimeOnly' }
  ];

  refreshTokenExpirationOptions = [
    { value: 0, text: 'Sliding' },
    { value: 1, text: 'Absolute' }
  ];

  accessTokenTypeOptions = [
    { value: 0, text: 'JWT' },
    { value: 1, text: 'Reference' }
  ];

  // Editor options objects
  refreshTokenUsageEditorOptions = {
    items: [
      { value: 0, text: 'ReUse' },
      { value: 1, text: 'OneTimeOnly' }
    ],
    displayExpr: 'text',
    valueExpr: 'value'
  };

  refreshTokenExpirationEditorOptions = {
    items: [
      { value: 0, text: 'Sliding' },
      { value: 1, text: 'Absolute' }
    ],
    displayExpr: 'text',
    valueExpr: 'value'
  };

  accessTokenTypeEditorOptions = {
    items: [
      { value: 0, text: 'JWT' },
      { value: 1, text: 'Reference' }
    ],
    displayExpr: 'text',
    valueExpr: 'value'
  };

  // Multi-select items
  selectedGrantTypes: string[] = [];
  selectedScopes: string[] = [];
  redirectUris: string[] = [];
  postLogoutRedirectUris: string[] = [];
  allowedCorsOrigins: string[] = [];
  identityProviderRestrictions: string[] = [];

  // Available scopes (these should be fetched from API in production)
  availableScopes = [
    'openid',
    'profile',
    'email',
    'address',
    'phone',
    'offline_access',
    'api_admin',
    'api_user'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private clientsService: ClientsService
  ) {}

  ngOnInit() {
    this.clientId = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!this.clientId;

    if (this.isEditMode) {
      this.loadClient();
    }
  }

  async loadClient() {
    if (!this.clientId) return;

    this.isLoading = true;
    try {
      const client = await this.clientsService.dataSource.store().byKey(this.clientId);
      if (client) {
        this.client = client;
        this.selectedGrantTypes = client.allowedGrantTypes || [];
        this.selectedScopes = client.allowedScopes || [];
        this.redirectUris = client.redirectUris || [];
        this.postLogoutRedirectUris = client.postLogoutRedirectUris || [];
        this.allowedCorsOrigins = client.allowedCorsOrigins || [];
        this.identityProviderRestrictions = client.identityProviderRestrictions || [];
      }
    } catch (error) {
      notify('Failed to load client', 'error', 3000);
      console.error('Error loading client:', error);
    } finally {
      this.isLoading = false;
    }
  }

  async onSave() {
    // Validate required fields
    if (!this.client.clientId || !this.client.clientName) {
      notify('Client ID and Client Name are required', 'warning', 3000);
      return;
    }

    this.isSaving = true;
    try {
      // Update arrays from UI
      this.client.allowedGrantTypes = this.selectedGrantTypes;
      this.client.allowedScopes = this.selectedScopes;
      this.client.redirectUris = this.redirectUris;
      this.client.postLogoutRedirectUris = this.postLogoutRedirectUris;
      this.client.allowedCorsOrigins = this.allowedCorsOrigins;
      this.client.identityProviderRestrictions = this.identityProviderRestrictions;

      if (this.isEditMode) {
        await this.clientsService.dataSource.update(this.client.id!, this.client);
        notify('Client updated successfully', 'success', 2000);
      } else {
        await this.clientsService.dataSource.insert(this.client);
        notify('Client created successfully', 'success', 2000);
      }

      this.router.navigate(['/clients']);
    } catch (error) {
      notify(`Failed to ${this.isEditMode ? 'update' : 'create'} client`, 'error', 3000);
      console.error('Error saving client:', error);
    } finally {
      this.isSaving = false;
    }
  }

  async onCancel() {
    const result = await confirm(
      'Are you sure you want to cancel? Any unsaved changes will be lost.',
      'Confirm Cancel'
    );

    if (result) {
      this.router.navigate(['/clients']);
    }
  }

  // URI list management
  onAddRedirectUri(uri: string) {
    if (uri && !this.redirectUris.includes(uri)) {
      this.redirectUris.push(uri);
    }
  }

  onRemoveRedirectUri(uri: string) {
    this.redirectUris = this.redirectUris.filter(u => u !== uri);
  }

  onAddPostLogoutUri(uri: string) {
    if (uri && !this.postLogoutRedirectUris.includes(uri)) {
      this.postLogoutRedirectUris.push(uri);
    }
  }

  onRemovePostLogoutUri(uri: string) {
    this.postLogoutRedirectUris = this.postLogoutRedirectUris.filter(u => u !== uri);
  }

  onAddCorsOrigin(origin: string) {
    if (origin && !this.allowedCorsOrigins.includes(origin)) {
      this.allowedCorsOrigins.push(origin);
    }
  }

  onRemoveCorsOrigin(origin: string) {
    this.allowedCorsOrigins = this.allowedCorsOrigins.filter(o => o !== origin);
  }

  private getEmptyClient(): Client {
    return {
      clientId: '',
      clientName: '',
      description: '',
      clientUri: '',
      logoUri: '',
      enabled: true,
      protocolType: 'oidc',
      requireClientSecret: true,
      requirePkce: false,
      allowPlainTextPkce: false,
      requireConsent: false,
      allowRememberConsent: true,
      alwaysIncludeUserClaimsInIdToken: false,
      requireRequestObject: false,
      allowedGrantTypes: [],
      redirectUris: [],
      postLogoutRedirectUris: [],
      frontChannelLogoutUri: '',
      frontChannelLogoutSessionRequired: true,
      backChannelLogoutUri: '',
      backChannelLogoutSessionRequired: true,
      allowOfflineAccess: false,
      allowedScopes: [],
      allowedCorsOrigins: [],
      identityTokenLifetime: 300,
      accessTokenLifetime: 3600,
      authorizationCodeLifetime: 300,
      absoluteRefreshTokenLifetime: 2592000,
      slidingRefreshTokenLifetime: 1296000,
      consentLifetime: null,
      refreshTokenUsage: 1,
      updateAccessTokenClaimsOnRefresh: false,
      refreshTokenExpiration: 1,
      accessTokenType: 0,
      enableLocalLogin: true,
      identityProviderRestrictions: [],
      includeJwtId: true,
      alwaysSendClientClaims: false,
      clientClaimsPrefix: 'client_',
      pairWiseSubjectSalt: '',
      userSsoLifetime: null,
      userCodeType: '',
      deviceCodeLifetime: 300,
      allowAccessTokensViaBrowser: false,
      created: new Date(),
      updated: null,
      lastAccessed: null,
      nonEditable: false
    };
  }
}
