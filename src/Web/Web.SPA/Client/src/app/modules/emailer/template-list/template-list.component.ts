import { Component, HostListener, OnInit, inject } from '@angular/core';
import { TemplatesDataSource } from '../data/templates.data-source';
import dxDataGrid, { RowDblClickEvent } from 'devextreme/ui/data_grid';
import { PopupService } from '@app/core/services/popup.service';
import { TemplateEditorComponent, TemplateEditorPopupContext, TemplateEditorPopupData } from '../template-editor/template-editor.component';
import { DistributionListComponent, DistributionListPopupContext, DistributionListPopupData } from '../distribution-list/distribution-list.component';
import { Subject } from 'rxjs';
import { SenderListComponent, SenderListPopupContext, SenderListPopupData } from '../sender-list/sender-list.component';

@Component({
  selector: 'hbg-template-list',
  templateUrl: './template-list.component.html',
  styleUrls: ['./template-list.component.scss']
})
export class TemplateListComponent {
  constructor() {}

  public ds: TemplatesDataSource = inject(TemplatesDataSource);
  public popups: PopupService = inject(PopupService);
  public grid: dxDataGrid;
  public selectedRowKey$ = new Subject<number>();

  @HostListener("document:keyup", ["$event"])
  handleKeyboardEvent(e) {
    if (e.key === "Enter" && this.grid.hasEditData())
      this.grid.saveEditData();
  }

  public onOpenEditor(e: RowDblClickEvent) {
    const id = e.data.templateID;
    this.popups.pushPopup<TemplateEditorComponent, TemplateEditorPopupData>(new TemplateEditorPopupContext({templateID: id})).subscribe();
  }

  public onOpenDistributions() {
    const id = this.grid.getSelectedRowKeys()[0];
    if (!id) return;
    this.popups.pushPopup<DistributionListComponent, DistributionListPopupData>(new DistributionListPopupContext({ templateID: id })).subscribe();
  }

  public onOpenSenders() {
    this.popups.pushPopup<SenderListComponent, SenderListPopupData>(new SenderListPopupContext({})).subscribe();
  }
}
