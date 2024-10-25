import { Component, HostListener, inject } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { BehaviorSubject } from 'rxjs';
import { DistributionsDataSource } from '../data/distributions.data-source';
import dxDataGrid, { InitNewRowEvent } from 'devextreme/ui/data_grid';
import { PopupService } from '@app/core/services/popup.service';
import { Properties as SelectBoxOptions } from 'devextreme/ui/select_box';
import { SendersDataSource } from '../data/sender.data-source';
import { SenderListComponent, SenderListPopupContext, SenderListPopupData } from '../sender-list/sender-list.component';
import { Distribution } from '../models/distribution.model';
import { EmailingReceiverListComponent, EmailingReceiverListPopupContext, EmailingReceiverListPopupData } from '../emailing-receivers-list/emailing-receiver-list.component';

export interface DistributionListPopupData { templateID: number; }

export class DistributionListPopupContext extends PopupContext<DistributionListComponent, DistributionListPopupData> {
  constructor(data: DistributionListPopupData) { super(data); }

  public fullScreen: boolean = true;
  public showTitle: boolean = false;
  public component = DistributionListComponent;
  public toolbar: PopupToolbarItem[] = [
    {
      id: ToolbarID.Cancel,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Close",
        onClick: () => this.closed$.next({ data: null, closedBy: ToolbarID.Cancel })
      }
    }
  ]
}

@Component({
  selector: 'hbg-distribution-list',
  templateUrl: './distribution-list.component.html',
  styleUrls: ['./distribution-list.component.scss']
})
export class DistributionListComponent implements IPopup<DistributionListPopupData>  {
  constructor() { }
  public ds: DistributionsDataSource = inject(DistributionsDataSource);
  public sendersDs: SendersDataSource = inject(SendersDataSource);
  public popups: PopupService = inject(PopupService);
  
  public readonly data$ = new BehaviorSubject<DistributionListPopupContext>(undefined);

  public grid: dxDataGrid;
  public selectedRowKey$ = new BehaviorSubject<number>(undefined);

  @HostListener("document:keyup", ["$event"])
  handleKeyboardEvent(e) {
    if (e.key === "Enter" && this.grid?.hasEditData())
      this.grid.saveEditData();
  }
  
  public senderEditorOptions: SelectBoxOptions = {
    dataSource: this.sendersDs,
    valueExpr: "senderID",
    displayExpr: "name",
    searchEnabled: true,
    searchMode: "startswith",
    buttons: [{
      name: "senderSelector",
      location: "after",
      options: {
        icon: "add",
        type: "default",
        stylingMode: "text",
        onClick: () => {
          this.popups.pushPopup<SenderListComponent, SenderListPopupData>(new SenderListPopupContext({})).subscribe();
        }
      }
    }]
  }

  public onInitNewRow(e: InitNewRowEvent<Distribution, number> ) {
    if (!e.data.templateID)
      e.data.templateID = this.data$.value.data.templateID;
  }

  public onOpenEmails() {
    this.popups.pushPopup<EmailingReceiverListComponent, EmailingReceiverListPopupData>(new EmailingReceiverListPopupContext({distributionID: this.selectedRowKey$.value})).subscribe();
  }

  public onStartDistribution = () => this.ds.startDistribution(this.selectedRowKey$.value);
}
