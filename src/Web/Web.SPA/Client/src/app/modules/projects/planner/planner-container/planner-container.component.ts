import { Component, inject, Input, OnInit } from '@angular/core';
import { CutImageService } from '@app/core/services/cut-image.service';
import { BehaviorSubject, filter, firstValueFrom, map, Observable, of, startWith, switchMap, tap } from 'rxjs';
import { Plan } from '../../models/plan.model';
import dxFileUploader from 'devextreme/ui/file_uploader';
import { PlansWsDataSource } from '../../data/plans-ws.data-source';
import { ImagesDataSource } from '../../data/images.data-source';
import { CutImagePopupData } from '@app/core/components/image-cutter/cut-image-popup/cut-image-popup.component';
import { CoordinatesService } from '../services/coordinates.service';
import { latLng } from 'leaflet';

enum PlannerMode {
  Empty,
  Uploader,
  Main
}

@Component({
  selector: 'hbg-planner-container',
  templateUrl: './planner-container.component.html',
  styleUrls: ['./planner-container.component.scss']
})
export class PlannerContainerComponent {
  private readonly cutter = inject(CutImageService);
  private readonly ds = inject(PlansWsDataSource);
  private readonly images = inject(ImagesDataSource);
  private readonly crdnts = inject(CoordinatesService);
  constructor() { }

  public readonly plan$ = this.ds.selected$.pipe(
    filter(x => x != null),
    tap(x => console.log('SELECTED', x)),
  );

  public readonly PlannerMode = PlannerMode;

  public readonly mode$ = this.plan$.pipe(
    map(p => p.hasPlanPicture ? PlannerMode.Main : PlannerMode.Uploader),
    startWith(PlannerMode.Empty)
  );
  // public readonly isReadonly$ = new BehaviorSubject<boolean>(false);
  public readonly isReadOnly$ = this.plan$.pipe(map(p => p.isReadOnly));
  
  public readonly uploaderComponent$ = new BehaviorSubject<dxFileUploader>(undefined);

  public readonly uploadFile = (file: File, progressCallback: Function) => firstValueFrom(this.cutter
    .cutImage(file).pipe(
      switchMap((f) => this.uploadAction(f)),
      // patch the model
      switchMap((data) => this.ds.patchPositionCreated(data, true)),
      // refresh selection
      switchMap(() => this.ds.refresh()),
      map(() => true)
    )
  );



  private readonly uploadAction = (data: CutImagePopupData) : Observable<any> => {
    if (!data.blob) {
      // reset widget => no 'uploaded' event
      this.uploaderComponent$.value.reset();
      return of(data.blob);
    }
    const model: Plan = this.ds.selected();
    return this.images.post$(model, data.blob as File).pipe(map(() => data));
  }

}
