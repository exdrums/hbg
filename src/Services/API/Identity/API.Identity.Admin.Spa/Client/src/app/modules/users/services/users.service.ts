import { Injectable } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import DataSource from 'devextreme/data/data_source';
import { RestDataStore } from '@app/core/data/rest-data-store';
import { User } from '../models/user.model';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class UsersService {
  dataSource: DataSource;
  private dataStore: RestDataStore<User, string>;

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {
    const baseUrl = `${this.config.hbgidentityadminapi}/api/users`;

    this.dataStore = new RestDataStore<User, string>(this.http, {
      key: 'id',
      loadUrl: baseUrl,
      insertUrl: baseUrl,
      updateUrl: baseUrl,
      removeUrl: baseUrl
    }, 'users');

    this.dataSource = new DataSource({
      store: this.dataStore,
      reshapeOnPush: true
    });
  }

  refresh() {
    this.dataSource.reload();
  }
}
