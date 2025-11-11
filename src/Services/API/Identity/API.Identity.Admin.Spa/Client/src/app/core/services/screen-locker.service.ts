import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, OperatorFunction } from "rxjs";
import { finalize, tap } from "rxjs/operators";
import { BaseService } from "./base/base.service";

@Injectable()
export class ScreenLockerService extends BaseService {
  constructor() {
    super();
  }

  /**
   * Array of screen lockers
   */
  private readonly lockers: string[] = [];
  public readonly locked$: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  public lock(locker: string) {
    this.lockers.push(locker);
    this.locked$.next(true);
  }

  public unlock(locker: string) {
    if (!this.lockers.includes(locker)) return;
    this.lockers.splice(this.lockers.indexOf(locker), 1);
    this.locked$.next(this.lockers.length > 0);
  }

  public lockPipe$<T>(): OperatorFunction<T, T> {
    const locker = Math.random().toString(36).substring(7);
    return (input$: Observable<T>) => input$.pipe(
      tap(() => this.lock(locker)),
      finalize(() => this.unlock(locker))
    )
  }
}
