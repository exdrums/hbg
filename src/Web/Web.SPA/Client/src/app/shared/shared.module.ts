import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserModule } from '@angular/platform-browser';
import { BaseComponent } from './base/base-component';
import { ViewsComponent } from './components/views/views.component';
import { HbgDxModule } from './hbg-dx.module';
import { ResizablesComponent } from './components/resizables/resizables.component';
import { TabsComponent } from './components/tabs/tabs.component';
import { SwitchableItemDirective } from './directives/switchable-item.directive';
import { ConnectionCheckComponent } from './components/connection-check/connection-check.component';
import { FormItemDescriptionDirective } from './directives/form-item-helper.directive';
import { FormItemDescComponent } from './components/form-item-desc/form-item-desc.component';

@NgModule({
  imports: [
    CommonModule,
    BrowserModule,

    HbgDxModule
  ],
  exports: [
    CommonModule,
    BrowserModule,
    HbgDxModule,
    
    ViewsComponent,
    TabsComponent,
    ResizablesComponent,

    FormItemDescComponent,
    FormItemDescriptionDirective,
    SwitchableItemDirective,
    ConnectionCheckComponent
  ],
  declarations: [
    ViewsComponent,
    TabsComponent,
    ResizablesComponent,
    BaseComponent,

    FormItemDescComponent,
    FormItemDescriptionDirective,
    SwitchableItemDirective,
    ConnectionCheckComponent
  ]
})
export class SharedModule { }
