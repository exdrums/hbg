import { inject, Injectable } from '@angular/core';
import { ProjectsWsDataSource } from '../../data/projects-ws.data-source';
import { map, tap } from 'rxjs';
import { latLng } from 'leaflet';

@Injectable()
export class CoordinatesService {
  private readonly projectsDs = inject(ProjectsWsDataSource);
  constructor() { }
  
  public defaultCoordinates$ = this.projectsDs.selected$.pipe(
    tap(x => console.log('default-coordinate', x)),
    map(p => latLng(p.defaultCoorX, p.defaultCoorY))
  )

  public defaultPlaneScaleValue = 0.1;

}
