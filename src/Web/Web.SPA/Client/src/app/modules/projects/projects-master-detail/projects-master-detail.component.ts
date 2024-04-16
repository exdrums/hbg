import { Component, Input, OnInit } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'hbg-projects-master-detail',
  templateUrl: './projects-master-detail.component.html',
  styleUrls: ['./projects-master-detail.component.scss']
})
export class ProjectsMasterDetailComponent implements OnInit {
  constructor() { }
  ngOnInit() { }

  @Input() public projectId: number;
  // @Input() public set project(value) {
  //   console.log('MD_set_project', value);
  // }
  
  public tabsItems: any[] = ["Articles", "Plans"];

  public tabChanged(title: string) {}
}
