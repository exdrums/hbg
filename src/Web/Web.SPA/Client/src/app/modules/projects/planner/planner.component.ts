import { Component, Input, OnInit, inject } from '@angular/core';
import dxFileUploader from 'devextreme/ui/file_uploader';
import { BehaviorSubject, Observable, firstValueFrom, of, switchMap, tap } from 'rxjs';
import { Plan } from '../models/plan.model';
import { CutImageService } from '@app/core/services/cut-image.service';

enum PlannerMode {
  Empty,
  Uploader,
  Main
}

@Component({
  selector: 'hbg-planner',
  templateUrl: './planner.component.html',
  styleUrls: ['./planner.component.scss']
})
export class PlannerComponent {
  private readonly cutter = inject(CutImageService)
  constructor() { }

  public readonly plan$ = new BehaviorSubject<Plan>(undefined);
  @Input() public set plan(value: Plan) {
    this.plan$.next(value);
  }

  public readonly PlannerMode = PlannerMode;
  public readonly mode$ = new BehaviorSubject<PlannerMode>(PlannerMode.Uploader);
  public readonly isReadonly$ = new BehaviorSubject<boolean>(false);

  public readonly uploaderComponent$ = new BehaviorSubject<dxFileUploader>(undefined);

  public readonly uploadFile = (file: File) => firstValueFrom(this.cutter
    .cutImage(file).pipe(
      tap(x => console.log('CROPPRESULT', x)),
      switchMap((f) => this.uploadAction(f))
    )
  );

  private readonly uploadAction = (file: File) : Observable<any> => {
    if (!file) {
      // reset widget => no 'uploaded' event
      this.uploaderComponent$.value.reset();
      return of(file);
    }
    const formData = new FormData();
    formData.append("file", file);
    console.log('RESULT_PLAN', file, formData);
    return of(null);
    // return this.http.post(url, formData);
  }


}
