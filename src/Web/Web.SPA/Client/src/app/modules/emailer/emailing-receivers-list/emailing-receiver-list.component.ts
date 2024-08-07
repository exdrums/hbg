import { Component, inject, OnInit } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { notnull } from '@app/core/utils/rxjs';
import { BehaviorSubject, filter, map, switchMap, take, tap } from 'rxjs';
import { EmailingReceiversDataSource } from '../data/emailing-receivers.data-source';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '@app/core/services/config.service';
import dxDataGrid from 'devextreme/ui/data_grid';

export interface EmailingReceiverListPopupData { distributionID: number; }

export class EmailingReceiverListPopupContext extends PopupContext<EmailingReceiverListComponent, EmailingReceiverListPopupData> {
  constructor(data: EmailingReceiverListPopupData) { super(data); }

  public title = "Contacts receiving emails";
  public fullScreen: boolean = true;
  public showTitle: boolean = false;
  public component = EmailingReceiverListComponent;
  public toolbar: PopupToolbarItem[] = [
    {
      id: ToolbarID.Cancel,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Close",
        onClick: () => this.closed$.next({ data: null, closedBy: ToolbarID.Cancel })
      }
    },
    {
      id: ToolbarID.Ok,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Save",
        onClick: () => this.clicked$.next(ToolbarID.Ok)
      }
    }
  ]
}

@Component({
  selector: 'hbg-emailing-receiver-list',
  templateUrl: './emailing-receiver-list.component.html',
  styleUrls: ['./emailing-receiver-list.component.scss']
})
export class EmailingReceiverListComponent implements IPopup<EmailingReceiverListPopupData> {
  constructor() {
    this.saving$.subscribe();
  }
  private http: HttpClient = inject(HttpClient);
  private config: ConfigService = inject(ConfigService);

  public readonly data$ = new BehaviorSubject<EmailingReceiverListPopupContext>(undefined);

  public ds$ = this.data$.pipe(
    notnull(),
    map(d => new EmailingReceiversDataSource(this.config, this.http, d.data.distributionID))
  );

  public grid: dxDataGrid;

  public saving$ = this.data$.pipe(
    notnull(),
    take(1),
    switchMap(d => d.clicked$.pipe(
      filter(tid => tid === ToolbarID.Ok),
      switchMap(x => this.grid.saveEditData()),
      take(1),
      tap(() => this.data$.value.closed$.next({ data: null, closedBy: ToolbarID.Ok }))
    )),
  )
}
