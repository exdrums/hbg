import { Component, Input, OnInit, ViewContainerRef, ComponentFactoryResolver } from '@angular/core';
import { BaseComponent } from '../base/base.component';
import { IPopup, PopupContext } from '../popup-context';

@Component({
  selector: 'hbg-popup-content',
  template: '',
  styleUrls: ['./popup-content.component.scss']
})
export class PopupContentComponent extends BaseComponent implements OnInit {
  constructor(
    private container: ViewContainerRef,
    private componentFactory: ComponentFactoryResolver
  ) { super(); }

  @Input() public readonly context: PopupContext;

  ngOnInit() {
    const componentInstance = this.container.createComponent(
      this.componentFactory.resolveComponentFactory(this.context.component)
    ).instance as IPopup<any>;

    componentInstance.data$.next(this.context);
    componentInstance.data = this.context;
  }
}
