import { Component, inject, OnInit } from '@angular/core';
import { ImagesDataSource } from '@app/modules/projects/data/images.data-source';
import { PlansWsDataSource } from '@app/modules/projects/data/plans-ws.data-source';
import { Control, DrawEvents, DrawOptions, latLng, Layer, LeafletEvent, MapOptions, tileLayer, Map, FeatureGroup, featureGroup, imageOverlay } from 'leaflet';
import { BehaviorSubject, filter, map, switchMap, tap } from 'rxjs';

@Component({
  selector: 'hbg-planner',
  templateUrl: './planner.component.html',
  styleUrls: ['./planner.component.scss']
})
export class PlannerComponent implements OnInit {
  private readonly ds = inject(PlansWsDataSource);
  private readonly images = inject(ImagesDataSource);
  constructor() { }
  ngOnInit() {
    this.onDrawCreated$.pipe(
      filter(x => x != null),
      tap(x => this.drawnItems.addLayer(x.layer))).subscribe();
  }
  
  public readonly options: MapOptions = {
    zoom: 15,
    center: latLng(46.879966, -121.676909)
  };

  public map$ = new BehaviorSubject<Map>(undefined)
  public readonly drawnItems: FeatureGroup = featureGroup();
  public readonly drawOptions: Control.DrawConstructorOptions = { edit: {featureGroup: this.drawnItems} };

  public readonly baseLayers: { [name: string]: Layer } = {
    "No map": tileLayer("", { maxZoom: 18, attribution: "..." }),
    "Open Street Map": tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { maxZoom: 18, attribution: "..." })
  };



  public onDrawCreated$ = new BehaviorSubject<DrawEvents.Created>(undefined);
  public readonly onDrawCreated = (e) => this.onDrawCreated$.next(e);
  public onClick(e: LeafletEvent) {
    console.log('onClick', e);
  }

  public onMapReady(e: Map) {
    console.log('onMapReady', e);
    this.map$.next(e);
  }
}
