import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { takeWhile, tap } from "rxjs/operators";
import notify from "devextreme/ui/notify";

@Injectable()
export class NotificationService {
  constructor() {}
  public newMessage$: Subject<string> = new Subject<string>();
  private subscription = this.newMessage$
    .pipe(
      takeWhile(() => this != null),
      tap((x) => notify(x, "success", 1000))
    ).subscribe();
}
