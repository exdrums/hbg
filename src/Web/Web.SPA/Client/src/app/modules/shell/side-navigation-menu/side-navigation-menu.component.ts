import {
  Component,
  NgModule,
  Output,
  Input,
  EventEmitter,
  ViewChild,
  ElementRef,
  AfterViewInit,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { NavigationItem } from '@app/core/app-navigation';
import { NavigationService } from '@app/core/services/navigation.service';
import { DxTreeViewModule, DxTreeViewComponent, DxTreeViewTypes } from 'devextreme-angular/ui/tree-view';
import * as events from 'devextreme/events';
import { Subscription } from 'rxjs';

@Component({
  selector: 'hbg-side-navigation-menu',
  templateUrl: './side-navigation-menu.component.html',
  styleUrls: ['./side-navigation-menu.component.scss'],
})
export class SideNavigationMenuComponent implements AfterViewInit, OnDestroy, OnInit {
  @ViewChild(DxTreeViewComponent, { static: true })
  menu!: DxTreeViewComponent;

  @Output()
  selectedItemChanged = new EventEmitter<DxTreeViewTypes.ItemClickEvent>();

  @Output()
  openMenu = new EventEmitter<any>();

  @Input()
  get compactMode() {
    return this._compactMode;
  }

  @Input()
  set selectedItem(value: String) {
    this._selectedItem = value;
    this.setSelectedItem();
  }

  get selectedItem(): String {
    return this._selectedItem;
  }

  set compactMode(val) {
    this._compactMode = val;

    if (!this.menu.instance) {
      return;
    }

    if (val) {
      this.menu.instance.collapseAll();
    } else {
      this.menu.instance.expandItem(this._selectedItem);
    }
  }

  private _selectedItem!: String;

  private _items!: Record<string, unknown>[];
  private navigationSubscription?: Subscription;

  get items() {
    return this._items || [];
  }

  private _compactMode = false;

  constructor(
    private elementRef: ElementRef,
    private navigationService: NavigationService
  ) { }

  ngOnInit() {
    // Subscribe to navigation items changes (filtered by permissions)
    this.navigationSubscription = this.navigationService.getNavigationItems$().subscribe(
      (navigationItems: NavigationItem[]) => {
        this._items = navigationItems.map((item) => {
          // Ensure path starts with /
          if (item.path && !(/^\//.test(item.path))) {
            item.path = `/${item.path}`;
          }
          return { ...item, expanded: !this._compactMode };
        });

        // Refresh the tree view if it's already initialized
        if (this.menu && this.menu.instance) {
          this.menu.instance.repaint();
          this.setSelectedItem();
        }
      }
    );
  }

  setSelectedItem() {
    if (!this.menu.instance) {
      return;
    }

    this.menu.instance.selectItem(this.selectedItem);
  }

  onItemClick(event: DxTreeViewTypes.ItemClickEvent) {
    this.selectedItemChanged.emit(event);
  }

  ngAfterViewInit() {
    this.setSelectedItem();
    events.on(this.elementRef.nativeElement, 'dxclick', (e: Event) => {
      this.openMenu.next(e);
    });
  }

  ngOnDestroy() {
    events.off(this.elementRef.nativeElement, 'dxclick');
    if (this.navigationSubscription) {
      this.navigationSubscription.unsubscribe();
    }
  }
}
