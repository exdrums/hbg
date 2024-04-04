import { Directive, Input, TemplateRef } from '@angular/core';

@Directive({
  selector: '[switchableItem]'
})
export class SwitchableItemDirective {
  public title: string;
  constructor(public template: TemplateRef<unknown>) { }

  @Input() public set switchableItem(title: string) {
    this.title = title;
  }
}
