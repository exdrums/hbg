import { Injectable } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import DataSource from 'devextreme/data/data_source';
import { RestDataStore } from '@app/core/data/rest-data-store';
import { Grant } from '../models/grant.model';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class GrantsService {
  dataSource: DataSource;
  private dataStore: RestDataStore<Grant, string>;

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {
    const baseUrl = `${this.config.hbgidentityadminapi}/api/persistedgrants`;

    this.dataStore = new RestDataStore<Grant, string>(this.http, {
      key: 'key',
      loadUrl: baseUrl,
      insertUrl: baseUrl,
      updateUrl: baseUrl,
      removeUrl: baseUrl
    }, 'persistedgrants');

    this.dataSource = new DataSource({
      store: this.dataStore,
      reshapeOnPush: true
    });
  }

  refresh() {
    this.dataSource.reload();
  }
}
