import { Injectable } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import DataSource from 'devextreme/data/data_source';
import { RestDataStore } from '@app/core/data/rest-data-store';
import { Role } from '../models/role.model';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class RolesService {
  dataSource: DataSource;
  private dataStore: RestDataStore<Role, string>;

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {
    const baseUrl = `${this.config.hbgidentityadminapi}/api/roles`;

    this.dataStore = new RestDataStore<Role, string>(this.http, {
      key: 'id',
      loadUrl: baseUrl,
      insertUrl: baseUrl,
      updateUrl: baseUrl,
      removeUrl: baseUrl
    }, 'roles');

    this.dataSource = new DataSource({
      store: this.dataStore,
      reshapeOnPush: true
    });
  }

  refresh() {
    this.dataSource.reload();
  }
}
