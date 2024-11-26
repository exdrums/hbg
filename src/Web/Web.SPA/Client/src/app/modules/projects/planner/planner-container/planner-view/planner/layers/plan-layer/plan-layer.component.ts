import { Component, inject, Input, OnInit } from '@angular/core';
import { PlansWsDataSource } from '@app/modules/projects/data/plans-ws.data-source';
import { Map, latLngBounds } from 'leaflet';
import { BehaviorSubject, debounceTime, filter, map, Observable, switchMap, takeUntil, tap } from 'rxjs';
import { ImagesDataSource } from '@app/modules/projects/data/images.data-source';
import { Plan } from '@app/modules/projects/models/plan.model';
import { PlanLayer } from './plan-layer';
import { BaseComponent } from '@app/shared/base/base-component';

@Component({
  selector: 'hbg-plan-layer',
  templateUrl: './plan-layer.component.html',
  styleUrls: ['./plan-layer.component.scss']
})
export class PlanLayerComponent extends BaseComponent {
  private readonly ds = inject(PlansWsDataSource);
  private readonly images = inject(ImagesDataSource);
  constructor() { super(); }
  @Input() set map(value: Map) {
    if (!value) return;
    this.map$.next(value);
  }
  private readonly map$ = new BehaviorSubject<Map>(undefined);

  ngOnInit() { }

  public layer$ = this.map$.pipe(
    filter(x => x != null),
    switchMap(mapObj => this.ds.selected$.pipe(
      switchMap(plan => this.images.getPlanImage$(plan).pipe(
        map(url => this.createImageOverlay(mapObj, url, plan)),
      ))
    ))
  );

  private createImageOverlay(mapObj: Map, url: string, plan: Plan) {
    const widthOffset = (plan.picWidth / 2) * plan.picScale;
    const heightOffset = (plan.picHeight / 2) * plan.picScale;

    const contCenter = mapObj.latLngToContainerPoint([plan.picCenterX, plan.picCenterY]);

    const contPicStart = contCenter.add([ -widthOffset, -heightOffset ]);
    const contPicEnd = contCenter.add([widthOffset, heightOffset]);
    const contPicStart2 = contCenter.add([widthOffset, -heightOffset]);
    const contPicEnd2 = contCenter.add([-widthOffset, heightOffset]);

    const absPicStart = mapObj.containerPointToLatLng(contPicStart);
    const absPicEnd = mapObj.containerPointToLatLng(contPicEnd);
    const absPicStart2 = mapObj.containerPointToLatLng(contPicStart2);
    const absPicEnd2 = mapObj.containerPointToLatLng(contPicEnd2);

    // console.log('Data', plan.picWidth, plan.picHeight, plan.picScale, plan.picCenterX, plan.picCenterY);
    // console.log('offsetes', widthOffset, heightOffset);
    // console.log('ContPoints', contPicStart, contPicEnd);
    // console.log("AbsPoints", absPicStart, absPicEnd);

    const overlay = new PlanLayer(
      url,
      latLngBounds([absPicStart, absPicStart2, absPicEnd, absPicEnd2]),
      {
        editable: true,
        interactive: true,
      },
      plan.picWidth,
      plan.picHeight
    );

    this.positionChanging$(overlay);

    return overlay; 
  }

  private readonly positionChanging$ = (layer: PlanLayer) => layer.positionChanged$.pipe(
    takeUntil(this.destroyed$),
    debounceTime(1000),
    tap(x => this.ds.patchPosition(x))
  ).subscribe();
}
