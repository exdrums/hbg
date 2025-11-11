import { BehaviorSubject, Subject } from 'rxjs';
import { Type } from '@angular/core';
import DxPopup, { InitializedEvent, ToolbarItem as dxPopupToolbarItem } from 'devextreme/ui/popup';

export abstract class PopupContext<T extends IPopup<Data> = any, Data = any> {
  constructor(public data: Data) { }

  public readonly dxPopupComponent$ = new BehaviorSubject<DxPopup>(undefined);
  public readonly closed$: Subject<PopupResult<Data>> = new Subject<PopupResult<Data>>();
  public readonly clicked$: Subject<ToolbarID> = new Subject<ToolbarID>();

  public width: string = "auto";
  public height: string = "auto";
  public maxWidth: string;
  public maxHeight: string;
  public minWidth: string;
  public minHeight: string;
  public dragEnabled: boolean = true;
  public resizeEnabled: boolean = true;
  public showTitle: boolean = true;
  public title: string = "Default_title";
  public fullScreen: boolean = false;
  public showCloseButton: boolean = false;

  public abstract component: Type<T>;
  public abstract toolbar: PopupToolbarItem[];

  public popupInitialised(popup: InitializedEvent) {
    this.dxPopupComponent$.next(popup.component);
  }

  public defaultClosed() {
    this.clicked$.next(ToolbarID.Cancel);
  }

  protected defaultOkCancelButtons: PopupToolbarItem[] = [
    {
      id: ToolbarID.Cancel,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Cancel",
        onClick: () => this.clicked$.next(ToolbarID.Cancel),
      },
    },
    {
      id: ToolbarID.Ok,
      location: "after",
      widget: "dxButton",
      options: {
        text: "OK",
        onClick: () => this.closed$.next({ data: this.data, closedBy: ToolbarID.Ok }),
      },
    }
  ];
}

export interface PopupResult<Data> {
  closedBy: ToolbarID;
  data: Data;
}

export interface IPopup<Data> {
  data$: Subject<PopupContext<IPopup<Data>, Data>>;
  data?: PopupContext<IPopup<Data>, Data>;
}

export enum ToolbarID {
  Ok = 1,
  Cancel = 2
}

export interface PopupToolbarItem extends dxPopupToolbarItem {
  id: ToolbarID;
  disabled$?: Subject<boolean>;
  visible$?: Subject<boolean>;
}
