import { Injectable, OnDestroy } from "@angular/core";
import { Subject } from "rxjs";

@Injectable()
export class BaseService implements OnDestroy {
    public destroyed$ = new Subject();
    ngOnDestroy(): void {
      this.destroyed$.next({});
      this.destroyed$.complete();
    }
}