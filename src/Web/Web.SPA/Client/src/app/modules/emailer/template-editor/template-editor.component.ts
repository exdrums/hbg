import { Component, inject, OnInit } from '@angular/core';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { BehaviorSubject, filter, map, switchMap, take, tap } from 'rxjs';
import { TemplatesDataSource } from '../data/templates.data-source';
import dxHtmlEditor from 'devextreme/ui/html_editor';
import { Template } from '../models/template.model';

export interface TemplateEditorPopupData {
  templateID: number;
}

export class TemplateEditorPopupContext extends PopupContext<TemplateEditorComponent, TemplateEditorPopupData>{
  constructor(data: TemplateEditorPopupData) { super(data); }
  // public width = "400";
  // public height = "300";
  // public dragEnabled = false;
  // public resizeEnabled = false;
  public fullScreen: boolean = true;
  public showTitle: boolean = false;
  public component = TemplateEditorComponent;
  public toolbar: PopupToolbarItem[] = [
    {
      id: ToolbarID.Cancel,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Cancel",
        onClick: () => this.closed$.next({ data: null, closedBy: ToolbarID.Cancel })
      }
    },
    {
      id: ToolbarID.Ok,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Save",
        onClick: () => this.clicked$.next(ToolbarID.Ok)
      }
    }
  ]
}

@Component({
  selector: 'msd-template-editor',
  templateUrl: './template-editor.component.html',
  styleUrls: ['./template-editor.component.scss']
})
export class TemplateEditorComponent implements IPopup<TemplateEditorPopupData>, OnInit {
  constructor() { 
    this.saving$.subscribe();
  }

  public ds: TemplatesDataSource = inject(TemplatesDataSource);
  public component: dxHtmlEditor;
  private current: Template;

  public readonly data$ = new BehaviorSubject<TemplateEditorPopupContext>(undefined);
  public readonly content$ = this.data$.pipe(
    filter(x => x != null),
    switchMap(e => this.ds.getFull$(e.data.templateID)),
    tap(t => this.current = t),
    map(t => t.content)
  );

  public saving$ = this.data$.pipe(
    filter(x => x != null),
    take(1),
    switchMap(d => d.clicked$.pipe(
      filter(tid => tid === ToolbarID.Ok),
      map(() => ({ ...this.current, content: this.component.option("value") })),  
      switchMap(x => this.ds.store().update(x.templateID, x)),
      take(1),
      tap(() => this.data$.value.closed$.next({ data: null, closedBy: ToolbarID.Ok }))
    )),
  )

  ngOnInit() {}

}
