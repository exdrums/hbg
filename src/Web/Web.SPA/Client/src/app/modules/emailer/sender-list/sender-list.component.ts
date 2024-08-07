import { Component, inject } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { SendersDataSource } from '../data/sender.data-source';
import { BehaviorSubject } from 'rxjs';
import dxDataGrid from 'devextreme/ui/data_grid';

export interface SenderListPopupData {}

export class SenderListPopupContext extends PopupContext<SenderListComponent, SenderListPopupData>{
  constructor(data: SenderListPopupData) { super(data); }

  public fullScreen: boolean = false;
  public height: string = "400px"
  public width: string = "350px";
  public showTitle: boolean = true;
  public title: string = "Senders";
  public component = SenderListComponent;
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
  selector: 'hbg-sender-list',
  templateUrl: './sender-list.component.html',
  styleUrls: ['./sender-list.component.scss']
})
export class SenderListComponent implements IPopup<SenderListPopupData> {
  constructor() { }
  public ds: SendersDataSource = inject(SendersDataSource);

  public readonly data$ = new BehaviorSubject<SenderListPopupContext>(undefined);
  public grid: dxDataGrid;
}
