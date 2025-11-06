import { Component, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { BehaviorSubject, of } from 'rxjs';
import { filter, mergeMap, switchMap, takeUntil, tap } from 'rxjs/operators';
import { BaseComponent } from '../base/base.component';
import { PopupService } from '../../../services/popup.service';
import { PopupContext } from '../popup-context';

@Component({
  selector: 'hbg-popup-stack',
  templateUrl: './popup-stack.component.html',
  styleUrls: ['./popup-stack.component.scss'],
  changeDetection: ChangeDetectionStrategy.Default
})
export class PopupStackComponent extends BaseComponent implements OnInit {
  constructor(
    private readonly service: PopupService,
    private readonly trigger: ChangeDetectorRef
  ) { super(); }

  public readonly stack$ = new BehaviorSubject<PopupContext[]>([]);

  ngOnInit() {
    this.service.popup$.pipe(
      filter(c => c != null),
      mergeMap(context => {
        this.addToStack(context);
        this.trigger.detectChanges();
        return this.removeOnClosed$(context);
      }),
      takeUntil(this.destroyed$)
    ).subscribe();
  }

  private readonly addToStack = (context: PopupContext) =>
    this.stack$.next([...this.stack$.value, context]);

  private readonly removeOnClosed$ = (context: PopupContext) =>
    context.closed$.pipe(
      switchMap(() => context?.dxPopupComponent$?.value ?
        context.dxPopupComponent$.value.hide() : of(null)),
      tap(() => {
        const array = this.stack$.value;
        array.splice(array.indexOf(context), 1);
        this.stack$.next(array);
      })
    );
}
