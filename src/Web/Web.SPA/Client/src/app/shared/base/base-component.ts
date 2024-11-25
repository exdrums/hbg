import { Component, Directive, OnDestroy, OnInit } from '@angular/core';
import { Subject } from 'rxjs';

@Directive({selector: 'hbg-base'})
export class BaseComponent implements OnDestroy, OnInit {
  public initialized$ = new Subject();
  ngOnInit(): void {
    this.initialized$.next({});
    this.initialized$.complete();
  }

  public destroyed$ = new Subject();
  ngOnDestroy(): void {
    this.destroyed$.next({});
    this.destroyed$.complete();
  }
}
