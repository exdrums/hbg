import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridComponent } from 'devextreme-angular';
import { IdentityResourcesService } from '../services/identity-resources.service';
import { confirm } from 'devextreme/ui/dialog';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-identity-resources-list',
  templateUrl: './identity-resources-list.component.html',
  styleUrls: ['./identity-resources-list.component.scss'],
  providers: [IdentityResourcesService]
})
export class IdentityResourcesListComponent implements OnInit {
  @ViewChild(DxDataGridComponent, { static: false }) dataGrid: DxDataGridComponent;

  constructor(
    public identityResourcesService: IdentityResourcesService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.identityResourcesService.refresh();
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
      },
      {
        location: 'after',
        widget: 'dxButton',
        options: {
          icon: 'add',
          text: 'New Identity Resource',
          type: 'default',
          onClick: () => this.onAddClick()
        }
      }
    );
  }

  onAddClick() {
    this.router.navigate(['/identity-resources/new']);
  }

  onEditClick(e: any) {
    const resourceId = e.row.data.id;
    this.router.navigate(['/identity-resources', resourceId]);
  }

  async onDeleteClick(e: any) {
    const result = await confirm(
      `Are you sure you want to delete identity resource "${e.row.data.name}"?`,
      'Confirm Delete'
    );

    if (result) {
      try {
        await this.identityResourcesService.dataSource.store().remove(e.row.data.id);
        notify('Identity resource deleted successfully', 'success', 2000);
        this.loadData();
      } catch (error) {
        notify('Failed to delete identity resource', 'error', 2000);
      }
    }
  }

  onRowDblClick(e: any) {
    this.onEditClick(e);
  }
}
