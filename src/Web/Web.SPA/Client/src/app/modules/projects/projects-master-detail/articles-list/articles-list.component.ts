import { Component, HostListener, Input, OnInit, inject } from '@angular/core';
import { ArticlesWsDataSource } from '../../data/articles-ws.data-source';
import dxDataGrid from 'devextreme/ui/data_grid';

@Component({
  selector: 'hbg-articles-list',
  templateUrl: './articles-list.component.html',
  styleUrls: ['./articles-list.component.scss']
})
export class ArticlesListComponent {
  public ds: ArticlesWsDataSource = inject(ArticlesWsDataSource);

  constructor() { }

  public grid: dxDataGrid;
  @HostListener("document:keyup", ["$event"])
  handleKeyboardEvent(e) {
    if (e.key === "Enter" && this.grid.hasEditData()) {
      this.grid?.instance().saveEditData();
    }
  }

  @Input() public set projectId(value: number) {
    console.log('ArticlesList_projectId', value);
    this.ds.projectId = value;
  }

}
