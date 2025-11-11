import { Injectable } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import DataSource from 'devextreme/data/data_source';
import { RestDataStore } from '@app/core/data/rest-data-store';
import { HttpClient } from '@angular/common/http';
import { IdentityResource } from '../models/identity-resource.model';

@Injectable()
export class IdentityResourcesService {
  dataSource: DataSource;
  private dataStore: RestDataStore<IdentityResource, string>;

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {
    const baseUrl = `${this.config.hbgidentityadminapi}/api/identityresources`;

    this.dataStore = new RestDataStore(this.http, {
      key: 'id',
      loadUrl: baseUrl,
      insertUrl: baseUrl,
      updateUrl: baseUrl,
      removeUrl: baseUrl
    }, "identityResources");

    this.dataSource = new DataSource({
      store: this.dataStore,
      reshapeOnPush: true
    });
  }

  refresh() {
    this.dataSource.reload();
  }
}
