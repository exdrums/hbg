import { Directive, ElementRef, Host, HostListener, Inject, Input, OnDestroy, Renderer2, ViewContainerRef } from '@angular/core';
import { DxoLabelComponent } from 'devextreme-angular/ui/nested';
import dxPopover from 'devextreme/ui/popover';

// TODO: repair it
// doesnot work of form was rerendered (eg because of window size change)
@Directive({
  selector: '[popover-description]'
})
export class FormItemDescriptionDirective implements OnDestroy {
  constructor(
    private renderer: Renderer2,
    private host: DxoLabelComponent,
    private elementRef: ElementRef,
  ) {
    this.createIcon();
    this.createPopover();
    this.host.template = (data, cont) => {
      this.renderer.appendChild(cont, this.getLabel(data.text));
      this.renderer.appendChild(cont, this.iconElement);
      this.iconElement.parentElement.appendChild(this.popoverElement);
      // console.log('Template_popover-description', elementRef, this.host);
    };
    // console.log('CTOR_popover-description');
  }

  @Input() set 'popover-description'(value: string) {
    // console.log("INPUT_popover-description", value);
    this.descriptionElement = this.renderer.createElement('div');
    this.descriptionElement.innerHTML = value;
  }
  private iconElement: HTMLElement;
  private popoverElement: HTMLElement;
  private descriptionElement: HTMLElement;

  private getLabel(text: string) {
    const textElement = this.renderer.createElement("span");
    textElement.innerText = text;
    return textElement;
  }

  private createIcon() {
    this.iconElement = this.renderer.createElement('i');
    this.iconElement.innerText = '?';
    this.iconElement.className = 'field-description-icon';
  }

  private createPopover() {
    this.popoverElement = this.renderer.createElement('div');
    const popoverComponent = new dxPopover(this.popoverElement, {
      target: this.iconElement,
      maxWidth: "600px",
      position: "right",
      contentTemplate: () => this.descriptionElement,
      showEvent: {
        name: "mouseenter",
        delay: 100
      } ,
      hideEvent: {
        name: "mouseleave",
        delay: 500
      },

      animation: {
        show: {
          type: 'pop',
          from: { scale: 0 },
          to: { scale: 1 },
        },
        hide: {
          type: 'pop',
          from: { scale: 1 },
          to: { scale: 0 },
        },
      },
    });
  }

  ngOnDestroy(): void {
    // console.log('DESTROY_popover-description');
    this.iconElement?.remove();
    this.popoverElement?.remove();
  }
}
