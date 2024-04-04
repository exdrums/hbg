import { NgModule } from '@angular/core';
import { ProjectsComponent } from './projects.component';
import { SharedModule } from '@app/shared/shared.module';
import { ProjectsService } from './projects.service';
import { ProjectsWebSocketConntection } from './data/projects-ws-connection.service';
import { ProjectsWsDataSource } from './data/projects-ws.data-source';
import { ProjectsListComponent } from './projects-list/projects-list.component';

@NgModule({
  imports: [
    SharedModule
  ],
  declarations: [
    ProjectsComponent,
    ProjectsListComponent
  ],
  providers: [
    ProjectsService,
    ProjectsWebSocketConntection,
    ProjectsWsDataSource
  ]
})
export class ProjectsModule { }
