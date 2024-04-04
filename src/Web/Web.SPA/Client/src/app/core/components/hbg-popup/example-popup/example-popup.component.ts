import { Component, OnInit } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { BehaviorSubject, of, Subject } from 'rxjs';
import { filter, map } from 'rxjs/operators';

export interface ExamplePopupData {
  someInputData: string
}

export class ExamplePopupContext extends PopupContext<ExamplePopupComponent, ExamplePopupData> {
  constructor(data: ExamplePopupData) { super(data); }
  public width = "400";
  public height = "300";
  // public dragEnabled = false;
  // public resizeEnabled = false;
  public component = ExamplePopupComponent;
  public toolbar: PopupToolbarItem[] = [
    {
      id: ToolbarID.Cancel,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Cancel",
        onClick: () => this.clicked$.next(ToolbarID.Cancel)
      }
    },
    {
      id: ToolbarID.Ok,
      location: "after",
      widget: "dxButton",
      options: {
        text: "OK",
        onClick: () => this.closed$.next({data: this.data, closedBy: ToolbarID.Ok})
      }
    }
  ]
}

@Component({
  selector: 'hbg-example-popup',
  templateUrl: './example-popup.component.html',
  styleUrls: ['./example-popup.component.scss']
})
export class ExamplePopupComponent implements IPopup<ExamplePopupData>, OnInit {

  constructor() { }
  public readonly data$ = new BehaviorSubject<ExamplePopupContext>(undefined);
  public readonly text$ = this.data$.pipe(
    filter(x => x != null),
    map(d => d.data.someInputData)
  )
  ngOnInit() {
  }

}
