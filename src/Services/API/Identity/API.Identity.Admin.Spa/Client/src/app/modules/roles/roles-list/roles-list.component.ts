import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridComponent } from 'devextreme-angular';
import { RolesService } from '../services/roles.service';
import { confirm } from 'devextreme/ui/dialog';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-roles-list',
  templateUrl: './roles-list.component.html',
  styleUrls: ['./roles-list.component.scss'],
  providers: [RolesService]
})
export class RolesListComponent implements OnInit {
  @ViewChild(DxDataGridComponent, { static: false }) dataGrid: DxDataGridComponent;

  constructor(
    public rolesService: RolesService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.rolesService.refresh();
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
          text: 'New Role',
          type: 'default',
          onClick: () => this.onAddClick()
        }
      }
    );
  }

  onAddClick() {
    this.router.navigate(['/roles/new']);
  }

  onEditClick(e: any) {
    const roleId = e.row.data.id;
    this.router.navigate(['/roles', roleId]);
  }

  async onDeleteClick(e: any) {
    const result = await confirm(
      `Are you sure you want to delete role "${e.row.data.name}"?`,
      'Confirm Delete'
    );

    if (result) {
      try {
        await this.rolesService.dataSource.store().remove(e.row.data.id);
        notify('Role deleted successfully', 'success', 2000);
        this.loadData();
      } catch (error) {
        notify('Failed to delete role', 'error', 2000);
      }
    }
  }

  onRowDblClick(e: any) {
    this.onEditClick(e);
  }
}
