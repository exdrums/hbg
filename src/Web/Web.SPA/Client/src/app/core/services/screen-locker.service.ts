import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, OperatorFunction } from "rxjs";
import { finalize, tap } from "rxjs/operators";
import { BaseService } from "./base/base.service";
import { UUID } from 'angular2-uuid';


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
    // add new locker value
    this.lockers.push(locker);
    // lock the screen
    this.locked$.next(true);
  }

  public unlock(locker: string) {
    // this.lockers = this.lockers.filter(l => l === locker);
    if(!this.lockers.includes(locker)) return;
    // remove locker from list
    this.lockers.splice(this.lockers.indexOf(locker), 1);
    //push event
    this.locked$.next(this.lockers.length > 0);
  }

  public lockPipe$<T>(): OperatorFunction<T, T> {
    const locker = UUID.UUID();
    return (input$: Observable<T>) => input$.pipe(
      tap(() => this.lock(locker)),
      finalize(() => this.unlock(locker))
    )
  }
}
