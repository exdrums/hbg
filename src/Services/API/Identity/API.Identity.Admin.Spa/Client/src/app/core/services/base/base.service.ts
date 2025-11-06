import { OnDestroy } from '@angular/core';

export abstract class BaseService implements OnDestroy {
  protected destroyed = false;

  ngOnDestroy(): void {
    this.destroyed = true;
  }
}
