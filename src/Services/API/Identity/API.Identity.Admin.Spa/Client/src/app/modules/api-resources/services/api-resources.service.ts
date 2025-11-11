import { Injectable } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import DataSource from 'devextreme/data/data_source';
import { RestDataStore } from '@app/core/data/rest-data-store';
import { HttpClient } from '@angular/common/http';
import { ApiResource } from '../models/api-resource.model';

@Injectable()
export class ApiResourcesService {
  dataSource: DataSource;
  private dataStore: RestDataStore<ApiResource, string>;

  constructor(private http: HttpClient,private config: ConfigService) {
    const baseUrl = `${this.config.hbgidentityadminapi}/api/apiresources`;

    this.dataStore = new RestDataStore(this.http, {
      key: 'id',
      loadUrl: baseUrl,
      insertUrl: baseUrl,
      updateUrl: baseUrl,
      removeUrl: baseUrl
    }, "apiResources");

    this.dataSource = new DataSource({
      store: this.dataStore,
      reshapeOnPush: true
    });
  }

  refresh() {
    this.dataSource.reload();
  }
}
