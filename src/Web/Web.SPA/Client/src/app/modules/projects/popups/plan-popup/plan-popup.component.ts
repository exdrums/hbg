import { Component } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { BehaviorSubject } from 'rxjs';

export interface PlanPopupData {
  planID: number;
}

export class PlanPopupContext extends PopupContext<PlanPopupComponent, PlanPopupData> {
  public fullScreen: boolean = true;
  public component = PlanPopupComponent;
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
  selector: 'hbg-plan-popup',
  templateUrl: './plan-popup.component.html',
  styleUrls: ['./plan-popup.component.scss']
})
export class PlanPopupComponent implements IPopup<PlanPopupData> {
  constructor() { }

  public readonly data$ = new BehaviorSubject<PlanPopupContext>(undefined);

}
