import { Component, Input, OnInit } from '@angular/core';
import dxResizable from 'devextreme/ui/resizable';

@Component({
  selector: 'hbg-resizables',
  templateUrl: './resizables.component.html',
  styleUrls: ['./resizables.component.scss']
})
export class ResizablesComponent {
  constructor() { }
  @Input() verticalMode: boolean = false;

  public readonly onResize = (e:{ component?: dxResizable, element?: HTMLElement, model?: any, event?: any, width?: number, height?: number }) => {
    const parent = e.element.parentElement;
    const divTwo = e.element.nextElementSibling;

    if (this.verticalMode) {
      const remainingSpace = parent.clientHeight - e.height;
      divTwo.setAttribute("style", "height: " + remainingSpace + "px");
    } else {
      const remainingSpace = parent.clientWidth - e.width;
      divTwo.setAttribute("style", "width: " + remainingSpace + "px");
    }
	}

}
