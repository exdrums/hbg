import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridComponent } from 'devextreme-angular';
import { ApiResourcesService } from '../services/api-resources.service';
import { confirm } from 'devextreme/ui/dialog';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-api-resources-list',
  templateUrl: './api-resources-list.component.html',
  styleUrls: ['./api-resources-list.component.scss'],
  providers: [ApiResourcesService]
})
export class ApiResourcesListComponent implements OnInit {
  @ViewChild(DxDataGridComponent, { static: false }) dataGrid: DxDataGridComponent;

  constructor(
    public apiResourcesService: ApiResourcesService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.apiResourcesService.refresh();
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
          text: 'New API Resource',
          type: 'default',
          onClick: () => this.onAddClick()
        }
      }
    );
  }

  onAddClick() {
    this.router.navigate(['/api-resources/new']);
  }

  onEditClick(e: any) {
    const resourceId = e.row.data.id;
    this.router.navigate(['/api-resources', resourceId]);
  }

  async onDeleteClick(e: any) {
    const result = await confirm(
      `Are you sure you want to delete API resource "${e.row.data.name}"?`,
      'Confirm Delete'
    );

    if (result) {
      try {
        await this.apiResourcesService.dataSource.store().remove(e.row.data.id);
        notify('API resource deleted successfully', 'success', 2000);
        this.loadData();
      } catch (error) {
        notify('Failed to delete API resource', 'error', 2000);
      }
    }
  }

  onRowDblClick(e: any) {
    this.onEditClick(e);
  }
}
