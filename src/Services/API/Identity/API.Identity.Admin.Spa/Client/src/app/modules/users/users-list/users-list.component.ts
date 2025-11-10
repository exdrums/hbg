import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridComponent } from 'devextreme-angular';
import { UsersService } from '../services/users.service';
import { confirm } from 'devextreme/ui/dialog';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-users-list',
  templateUrl: './users-list.component.html',
  styleUrls: ['./users-list.component.scss'],
  providers: [UsersService]
})
export class UsersListComponent implements OnInit {
  @ViewChild(DxDataGridComponent, { static: false }) dataGrid: DxDataGridComponent;

  constructor(
    public usersService: UsersService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.usersService.refresh();
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
          text: 'New User',
          type: 'default',
          onClick: () => this.onAddClick()
        }
      }
    );
  }

  onAddClick() {
    this.router.navigate(['/users/new']);
  }

  onEditClick(e: any) {
    const userId = e.row.data.id;
    this.router.navigate(['/users', userId]);
  }

  async onDeleteClick(e: any) {
    const result = await confirm(
      `Are you sure you want to delete user "${e.row.data.username}"?`,
      'Confirm Delete'
    );

    if (result) {
      try {
        await this.usersService.dataSource.store().remove(e.row.data.id);
        notify('User deleted successfully', 'success', 2000);
        this.loadData();
      } catch (error) {
        notify('Failed to delete user', 'error', 2000);
      }
    }
  }

  onRowDblClick(e: any) {
    this.onEditClick(e);
  }
}
