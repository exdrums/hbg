import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '@app/core/services/config.service';
import { RestDataStore } from '@app/core/data/rest-data-store';
import { CustomDataSource } from '@app/core/data/custom-data-source';
import { Client } from '../models/client.model';

@Injectable()
export class ClientsService {
  private dataStore: RestDataStore<Client, string>;
  public dataSource: CustomDataSource<Client, string>;

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {
    const apiUrl = this.config.hbgidentityadminapi;

    this.dataStore = new RestDataStore<Client, string>(this.http, {
      key: 'id',
      loadUrl: `${apiUrl}/api/clients`,
      insertUrl: `${apiUrl}/api/clients`,
      updateUrl: `${apiUrl}/api/clients/`,
      removeUrl: `${apiUrl}/api/clients`
    });

    this.dataSource = new CustomDataSource<Client>(this.dataStore);
  }

  refresh() {
    return this.dataSource.reload();
  }
}
