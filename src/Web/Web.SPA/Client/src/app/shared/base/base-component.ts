import { Component, Directive, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';

@Directive({selector: 'hbg-base'})
export class BaseComponent implements OnDestroy {
  public destroyed$ = new Subject();
  ngOnDestroy(): void {
    this.destroyed$.next({});
    this.destroyed$.complete();
  }
}
