import { Component } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { BehaviorSubject } from 'rxjs';
import { Plan } from '../../models/plan.model';

export interface PlanPopupData {
  model: Plan
}

export class PlanPopupContext extends PopupContext<PlanPopupComponent, PlanPopupData> {
  public fullScreen: boolean = true;
  public component = PlanPopupComponent;
  public title: string = this.data.model.name;
  public showCloseButton = true;
  public toolbar: PopupToolbarItem[] = []
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
