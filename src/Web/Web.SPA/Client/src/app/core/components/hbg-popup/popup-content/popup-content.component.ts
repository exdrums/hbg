import { Component, ComponentFactoryResolver, Input, OnInit, ViewContainerRef } from '@angular/core';
import { BaseComponent } from '@app/shared/base/base-component';
import { IPopup, PopupContext } from '../popup-context';

@Component({
  selector: 'hbg-popup-content',
  templateUrl: './popup-content.component.html',
  styleUrls: ['./popup-content.component.scss']
})
export class PopupContentComponent extends BaseComponent implements OnInit {
  constructor(private container: ViewContainerRef, private componentFactory: ComponentFactoryResolver) { super();}
  @Input() public readonly context: PopupContext;

  ngOnInit() {
    // create component with factory
    const componentInstance = this.container.createComponent(this.componentFactory.resolveComponentFactory(this.context.component)).instance as IPopup<any>;
    // TODO: Wait for Popup (and toolbar?) are initialised => do you need?
    componentInstance.data$.next(this.context);
    componentInstance.data = this.context;
  }

}
