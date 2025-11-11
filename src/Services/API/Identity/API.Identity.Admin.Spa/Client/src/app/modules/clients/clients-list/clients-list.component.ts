import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridComponent } from 'devextreme-angular';
import { ClientsService } from '../services/clients.service';
import { confirm } from 'devextreme/ui/dialog';
import notify from 'devextreme/ui/notify';

@Component({
  selector: 'hbg-clients-list',
  templateUrl: './clients-list.component.html',
  styleUrls: ['./clients-list.component.scss'],
  providers: [ClientsService]
})
export class ClientsListComponent implements OnInit {
  @ViewChild(DxDataGridComponent, { static: false }) dataGrid: DxDataGridComponent;

  constructor(
    public clientsService: ClientsService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.clientsService.refresh();
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
          text: 'New Client',
          type: 'default',
          onClick: () => this.onAddClick()
        }
      }
    );
  }

  onAddClick() {
    this.router.navigate(['/clients/new']);
  }

  onEditClick(e: any) {
    const clientId = e.row.data.id;
    this.router.navigate(['/clients', clientId]);
  }

  async onDeleteClick(e: any) {
    const result = await confirm(
      `Are you sure you want to delete client "${e.row.data.clientName}"?`,
      'Confirm Delete'
    );

    if (result) {
      try {
        await this.clientsService.dataSource.remove(e.row.data.id);
        notify('Client deleted successfully', 'success', 2000);
      } catch (error) {
        notify('Failed to delete client', 'error', 2000);
      }
    }
  }

  onRowDblClick(e: any) {
    this.onEditClick(e);
  }
}
