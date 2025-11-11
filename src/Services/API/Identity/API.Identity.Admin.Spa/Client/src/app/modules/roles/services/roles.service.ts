import { Injectable } from '@angular/core';
import { ConfigService } from '@app/core/services/config.service';
import DataSource from 'devextreme/data/data_source';
import { RestDataStore } from '@app/core/services/rest.data.store';

@Injectable()
export class RolesService {
  dataSource: DataSource;
  private dataStore: RestDataStore;

  constructor(private config: ConfigService) {
    const baseUrl = `${this.config.hbgidentityadminapi}/api/roles`;

    this.dataStore = new RestDataStore({
      key: 'id',
      loadUrl: baseUrl,
      insertUrl: baseUrl,
      updateUrl: baseUrl,
      deleteUrl: baseUrl
    });

    this.dataSource = new DataSource({
      store: this.dataStore,
      reshapeOnPush: true
    });
  }

  refresh() {
    this.dataSource.reload();
  }
}
