import { Component, OnInit, ViewChild } from '@angular/core';
import { DxDataGridComponent } from 'devextreme-angular';
import { GrantsService } from '../services/grants.service';
import { confirm } from 'devextreme/ui/dialog';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-grants-list',
  templateUrl: './grants-list.component.html',
  styleUrls: ['./grants-list.component.scss'],
  providers: [GrantsService]
})
export class GrantsListComponent implements OnInit {
  @ViewChild(DxDataGridComponent, { static: false }) dataGrid: DxDataGridComponent;

  constructor(
    public grantsService: GrantsService
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.grantsService.refresh();
  }

  onToolbarPreparing(e: any) {
    e.toolbarOptions.items.unshift(
      {
        location: 'after',
        widget: 'dxButton',
        options: {
          icon: 'refresh',
          hint: 'Refresh',
          onClick: () => this.loadData()
        }
      }
    );
  }

  async onRevokeClick(e: any) {
    const result = await confirm(
      `Are you sure you want to revoke this grant for subject "${e.row.data.subjectId}"?`,
      'Confirm Revoke'
    );

    if (result) {
      try {
        await this.grantsService.dataSource.store().remove(e.row.data.key);
        notify('Grant revoked successfully', 'success', 2000);
        this.loadData();
      } catch (error) {
        notify('Failed to revoke grant', 'error', 2000);
      }
    }
  }
}
