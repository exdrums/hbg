import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { IPopup, PopupContext } from '../components/hbg-popup/popup-context';

@Injectable()
export class PopupService {
  constructor() { }
  public readonly popup$ = new BehaviorSubject<PopupContext>(undefined);

  /**Show popup component with given PopupContext */
  public pushPopup<T extends IPopup<Data>, Data>(context: PopupContext<T, Data>) {
    this.popup$.next(context);
    return context.closed$;
  }
}
