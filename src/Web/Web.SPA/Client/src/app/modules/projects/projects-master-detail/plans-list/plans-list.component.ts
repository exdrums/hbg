import { Component, HostListener, Input, OnInit, inject } from '@angular/core';
import { PlansWsDataSource } from '../../data/plans-ws.data-source';
import dxDataGrid from 'devextreme/ui/data_grid';

@Component({
  selector: 'hbg-plans-list',
  templateUrl: './plans-list.component.html',
  styleUrls: ['./plans-list.component.scss']
})
export class PlansListComponent {
  public ds: PlansWsDataSource = inject(PlansWsDataSource);

  constructor() { }
  public grid: dxDataGrid;
  @HostListener("document:keyup", ["$event"])
  handleKeyboardEvent(e) {
    if (e.key === "Enter" && this.grid.hasEditData()) {
      this.grid?.instance().saveEditData();
    }
  }

  @Input() public set projectId(value: number) {
    console.log('PlansList_projectId', value);
    this.ds.projectId = value;
  }

}
