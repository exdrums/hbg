import { Component, OnInit, inject } from '@angular/core';
import { ProjectsWsDataSource } from '../data/projects-ws.data-source';

@Component({
  selector: 'hbg-projects-list',
  templateUrl: './projects-list.component.html',
  styleUrls: ['./projects-list.component.scss']
})
export class ProjectsListComponent implements OnInit {
  public ds: ProjectsWsDataSource = inject(ProjectsWsDataSource);
  constructor() { }

  public onInitNewRow(p) {
    console.log('onInitNewRow', p);
  }

  ngOnInit() {}
}
