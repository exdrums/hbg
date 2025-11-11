import { Injectable, OnDestroy } from '@angular/core';

@Injectable()
export abstract class BaseService implements OnDestroy {
  protected destroyed = false;

  ngOnDestroy(): void {
    this.destroyed = true;
  }
}
