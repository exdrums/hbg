import { Injectable } from '@angular/core';
import { Project } from '../models/project.model';
import { CustomDataSource } from '@app/core/data/custom-data-source';
import { SignalRDataStore } from '@app/core/data/signalr-data-store';
import { ProjectsWebSocketConntection } from './projects-ws-connection.service';

@Injectable()
export class ProjectsWsDataSource extends CustomDataSource<Project> {
  constructor(private connection: ProjectsWebSocketConntection) {
    super(new SignalRDataStore<Project>(connection, "projectID", "Project"));
  }
  public readonly refresh = () => void this.reload();
}
