import { Component, HostListener, inject } from '@angular/core';
import { ProjectsWsDataSource } from '../data/projects-ws.data-source';
import dxDataGrid, { RowExpandedEvent, SelectionChangedEvent } from 'devextreme/ui/data_grid';

@Component({
  selector: 'hbg-projects-list',
  templateUrl: './projects-list.component.html',
  styleUrls: ['./projects-list.component.scss']
})
export class ProjectsListComponent {
  public ds: ProjectsWsDataSource = inject(ProjectsWsDataSource);

  constructor() { }

  public grid: dxDataGrid;
  @HostListener("document:keyup", ["$event"])
  handleKeyboardEvent(e) {
    if (e.key === "Enter" && this.grid.hasEditData()) {
      this.grid?.instance().saveEditData();
    }
  }

  public onInitNewRow(p) {
    console.log('onInitNewRow', p);
  }

  /**Enables single row open mode for master detail */
  public onRowExpanding(e: RowExpandedEvent) {
		this.grid?.collapseAll(-1);
		void this.grid?.selectRows([e.key], false);
  }

  public onSelectionChanged(e: SelectionChangedEvent) {
    if (!e.selectedRowsData[0]) return;
    this.ds.setSelected(e.selectedRowsData[0]);
  }
}
