import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '../popup-context';
import { BehaviorSubject } from 'rxjs';

export interface ErrorPopupData {
  serverError?: HttpErrorResponse,
  clientError?: Error
}

export class ErrorPopupContext extends PopupContext<PopupErrorComponent, ErrorPopupData>{
  constructor(data: ErrorPopupData) { super(data); }
  public width = "820";
  public height = "500";
  public title = "Error";
  // public dragEnabled = false;
  // public resizeEnabled = false;
  public component = PopupErrorComponent;
  public toolbar: PopupToolbarItem[] = [
    {
      id: ToolbarID.Cancel,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Close",
        onClick: () => this.closed$.next({data: this.data, closedBy: ToolbarID.Cancel})
      }
    }
  ]
}

@Component({
  selector: 'hbg-popup-error',
  templateUrl: './popup-error.component.html',
  styleUrls: ['./popup-error.component.scss']
})
export class PopupErrorComponent implements IPopup<ErrorPopupData> {
  constructor() { }
  public readonly data$ = new BehaviorSubject<ErrorPopupContext>(undefined);
}
