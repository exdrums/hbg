import { Component, HostListener, Input, OnInit, inject } from '@angular/core';
import { PlansWsDataSource } from '../../data/plans-ws.data-source';
import dxDataGrid, { ColumnButtonClickEvent, SelectionChangedEvent } from 'devextreme/ui/data_grid';
import { PopupService } from '@app/core/services/popup.service';
import { PlanPopupComponent, PlanPopupContext, PlanPopupData } from '../../popups/plan-popup/plan-popup.component';
import { Plan } from '../../models/plan.model';

@Component({
  selector: 'hbg-plans-list',
  templateUrl: './plans-list.component.html',
  styleUrls: ['./plans-list.component.scss']
})
export class PlansListComponent {
  public ds: PlansWsDataSource = inject(PlansWsDataSource);
  public popups: PopupService = inject(PopupService);

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

  public readonly openPlan = (e: ColumnButtonClickEvent) => {
    const model: Plan = e.row.data
    this.ds.setSelected(model);
    const data: PlanPopupData = { model };
    this.popups.pushPopup<PlanPopupComponent, PlanPopupData>(new PlanPopupContext(data)).subscribe();
  }

  public onSelectionChanged(e: SelectionChangedEvent) {
    const plan: Plan = e.selectedRowsData[0];
    if (!plan) return;
    this.ds.setSelected(plan);
  }

}
