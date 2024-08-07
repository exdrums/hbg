import { Component, HostListener, inject } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import dxDataGrid from 'devextreme/ui/data_grid';
import { BehaviorSubject } from 'rxjs';
import { ReceiversDataSource } from '../data/receivers.data-source';

export interface ReceiverListPopupData { }

export class ReceiverListPopupContext extends PopupContext<ReceiverListComponent, ReceiverListPopupData> {
  constructor(data: ReceiverListPopupData) { super(data); }

  public fullScreen: boolean = false;
  public height: string = "400px"
  public width: string = "350px";
  public showTitle: boolean = true;
  public title: string = "Receivers";
  public component = ReceiverListComponent;
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
  selector: 'hbg-receiver-list',
  templateUrl: './receiver-list.component.html',
  styleUrls: ['./receiver-list.component.scss']
})
export class ReceiverListComponent implements IPopup<ReceiverListPopupData> {
  constructor() { }
  public ds: ReceiversDataSource = inject(ReceiversDataSource);
  public readonly data$ = new BehaviorSubject<ReceiverListPopupContext>(undefined);
  public grid: dxDataGrid;

  @HostListener("document:keyup", ["$event"])
  handleKeyboardEvent(e) {
    if (e.key === "Enter" && this.grid?.hasEditData())
      this.grid.saveEditData();
  }
}
