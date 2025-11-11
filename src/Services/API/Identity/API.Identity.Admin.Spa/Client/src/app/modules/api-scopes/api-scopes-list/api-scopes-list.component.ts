import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridComponent } from 'devextreme-angular';
import { ApiScopesService } from '../services/api-scopes.service';
import { confirm } from 'devextreme/ui/dialog';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-api-scopes-list',
  templateUrl: './api-scopes-list.component.html',
  styleUrls: ['./api-scopes-list.component.scss'],
  providers: [ApiScopesService]
})
export class ApiScopesListComponent implements OnInit {
  @ViewChild(DxDataGridComponent, { static: false }) dataGrid: DxDataGridComponent;

  constructor(
    public apiScopesService: ApiScopesService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.apiScopesService.refresh();
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
          text: 'New API Scope',
          type: 'default',
          onClick: () => this.onAddClick()
        }
      }
    );
  }

  onAddClick() {
    this.router.navigate(['/api-scopes/new']);
  }

  onEditClick(e: any) {
    const scopeId = e.row.data.id;
    this.router.navigate(['/api-scopes', scopeId]);
  }

  async onDeleteClick(e: any) {
    const result = await confirm(
      `Are you sure you want to delete API scope "${e.row.data.name}"?`,
      'Confirm Delete'
    );

    if (result) {
      try {
        await this.apiScopesService.dataSource.store().remove(e.row.data.id);
        notify('API scope deleted successfully', 'success', 2000);
        this.loadData();
      } catch (error) {
        notify('Failed to delete API scope', 'error', 2000);
      }
    }
  }

  onRowDblClick(e: any) {
    this.onEditClick(e);
  }
}
