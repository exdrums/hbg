import { NgModule } from '@angular/core';
import { ProjectsComponent } from './projects.component';
import { SharedModule } from '@app/shared/shared.module';
import { ProjectsService } from './projects.service';
import { ProjectsWebSocketConntection } from './data/projects-ws-connection.service';
import { ProjectsWsDataSource } from './data/projects-ws.data-source';
import { ProjectsListComponent } from './projects-list/projects-list.component';
import { ProjectsMasterDetailComponent } from './projects-master-detail/projects-master-detail.component';
import { PlansListComponent } from './projects-master-detail/plans-list/plans-list.component';
import { ArticlesListComponent } from './projects-master-detail/articles-list/articles-list.component';
import { PlansWsDataSource } from './data/plans-ws.data-source';
import { ArticlesWsDataSource } from './data/articles-ws.data-source';
import { PlanPopupComponent } from './popups/plan-popup/plan-popup.component';
import { LeafletModule } from '@bluehalo/ngx-leaflet';
import { LeafletDrawModule } from '@bluehalo/ngx-leaflet-draw';
import { PlannerComponent } from './planner/planner-container/planner-view/planner/planner.component';
import { PlannerContainerComponent } from './planner/planner-container/planner-container.component';
import { PlannerViewComponent } from './planner/planner-container/planner-view/planner-view.component';
import { PlannerControlsComponent } from './planner/planner-container/planner-view/planner-controls/planner-controls.component';
import { PlanLayerComponent } from './planner/planner-container/planner-view/planner/layers/plan-layer/plan-layer.component';
import { ArticleIconsLayerComponent } from './planner/planner-container/planner-view/planner/layers/article-icons-layer/article-icons-layer.component';
import { DrawLayerComponent } from './planner/planner-container/planner-view/planner/layers/draw-layer/draw-layer.component';
import { ImagesDataSource } from './data/images.data-source';
import { CoordinatesService } from './planner/services/coordinates.service';

@NgModule({
  imports: [
    SharedModule,
    LeafletModule,
    LeafletDrawModule
  ],
  declarations: [
    ProjectsComponent,
    ProjectsListComponent,
    ProjectsMasterDetailComponent,
    PlanPopupComponent,
    PlansListComponent,
    PlannerContainerComponent,
    PlannerViewComponent,
    PlannerControlsComponent,
    PlannerComponent,
    PlanLayerComponent,
    ArticleIconsLayerComponent,
    DrawLayerComponent,
    ArticlesListComponent
  ],
  providers: [
    ProjectsService,
    CoordinatesService,
    ProjectsWebSocketConntection,
    ProjectsWsDataSource,
    PlansWsDataSource,
    ArticlesWsDataSource,
    ImagesDataSource
  ]
})
export class ProjectsModule { }
