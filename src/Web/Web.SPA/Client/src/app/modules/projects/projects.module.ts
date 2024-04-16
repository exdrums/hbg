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

@NgModule({
  imports: [
    SharedModule
  ],
  declarations: [
    ProjectsComponent,
    ProjectsListComponent,
    ProjectsMasterDetailComponent,
    PlansListComponent,
    ArticlesListComponent
  ],
  providers: [
    ProjectsService,
    ProjectsWebSocketConntection,
    ProjectsWsDataSource,
    PlansWsDataSource,
    ArticlesWsDataSource
  ]
})
export class ProjectsModule { }
