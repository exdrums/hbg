import { Component, ContentChildren, EventEmitter, Output, QueryList } from '@angular/core';
import { SwitchableItemDirective } from '@app/shared/directives/switchable-item.directive';

@Component({
  selector: 'hbg-tabs',
  templateUrl: './tabs.component.html',
  styleUrls: ['./tabs.component.scss']
})
export class TabsComponent {
  constructor() { }

  @Output() public tabChanged = new EventEmitter<string>();
  @ContentChildren(SwitchableItemDirective) public items: QueryList<SwitchableItemDirective>;
}
