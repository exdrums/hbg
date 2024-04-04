import { AfterViewInit } from '@angular/core';
import { Component, Input } from '@angular/core';
import { BaseComponent } from '@app/shared/base/base-component';
import { View, ViewIndex, ViewSettings } from '@app/shared/models/view.model';
import dxMultiView from 'devextreme/ui/multi_view';
import { Subject } from 'rxjs';
import { exhaustMap, take, takeUntil, tap } from 'rxjs/operators';

@Component({
  selector: 'hbg-views',
  templateUrl: './views.component.html',
  styleUrls: ['./views.component.scss']
})
export class ViewsComponent extends BaseComponent implements AfterViewInit {
  constructor() { super(); }

  //#region Properties


  @Input() v1: ViewSettings;
  @Input() v2: ViewSettings;
  @Input() v3: ViewSettings;
  private views: View[];

  public items: ViewIndex[] = ['v1', 'v2', 'v3'];

  //#endregion

  //#region Init

  private dxMultiView: dxMultiView;
  onInitialized(e: dxMultiView) {
    this.dxMultiView = e;
  }

  ngAfterViewInit(){
    this.views = [this.v1.view, this.v2.view, this.v3.view]
    this.initBinding(this.v1);
    this.initBinding(this.v2);
    this.initBinding(this.v3);
  }

  /**
   * Bind Subject-View with all their outputs
   * @param s 
   */
  private initBinding(s: ViewSettings) {
    this.connect(s.view, s.viewOutputTo);
    if(s.viewOutput2To)
    {
      if(!s.view.viewOutput2$)
        return console.error("Second output in Subject view is not implemented.");
      this.connect(s.view, s.viewOutput2To, s.view.viewOutput2$);
    }
  }

  private connect(subjectView: View, targetView: View, output?: Subject<any>, input?: Subject<any>) {
    if(!subjectView || !subjectView.viewOutput$ || !targetView || !targetView.viewInput$ || !targetView.viewBack$)
      return console.error("Cannot bind views. Not all properties are initialized", subjectView, targetView);
    // console.log('ConnectWith', output, subjectView.viewOutput$);
    (output || subjectView.viewOutput$).pipe(
      // tap(x => console.log('LOG Output', x, "from: ", subjectView, "to: ", targetView)),
      exhaustMap(value => {
        this.selectItem(targetView);
        (input || targetView.viewInput$).next(value);
        return targetView.viewBack$.pipe(
          take(1),
          // back was emitted
          tap(() => this.selectItem(subjectView))
        );
      }),
      takeUntil(this.destroyed$)
    ).subscribe();
  }

  /**
   * Select item in DxMultiViewComponent by passing View component
   * @param view View component
   */
  private selectItem(view: View) {
    const targetIndex = this.views.indexOf(view);
    if(targetIndex < 0) return console.error("Cannot select this item in DxMultiView component. View was not found.");

    const item = this.items[targetIndex];
    if(!item) return console.error("Cannot select this item in DxMultiView component. Item was not found.");

    this.dxMultiView.option("selectedItem", item);
  }

  //#endregion

  //#region Public Actions


  //#endregion

}
