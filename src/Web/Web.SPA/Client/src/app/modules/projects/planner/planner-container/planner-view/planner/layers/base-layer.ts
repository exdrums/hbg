import { Directive, inject, Input } from "@angular/core";
import { ArticlesWsDataSource } from "@app/modules/projects/data/articles-ws.data-source";
import { ImagesDataSource } from "@app/modules/projects/data/images.data-source";
import { PlansWsDataSource } from "@app/modules/projects/data/plans-ws.data-source";
import { BaseComponent } from "@app/shared/base/base-component";
import { Map, latLngBounds } from 'leaflet';
import { BehaviorSubject } from "rxjs";


@Directive({ selector: 'hbg-base' })
export class BaseLayerComponent extends BaseComponent {
    protected readonly plansDataSource = inject(PlansWsDataSource);
    protected readonly articlesDataSource = inject(ArticlesWsDataSource);
    protected readonly imagesDataSource = inject(ImagesDataSource);

    constructor() { super(); }
    @Input() set map(value: Map) {
        if (!value) return;
        this.map$.next(value);
      }
    protected readonly map$ = new BehaviorSubject<Map>(undefined);
}