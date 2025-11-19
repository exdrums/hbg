import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

// DevExtreme Modules
import {
  DxButtonModule,
  DxDataGridModule,
  DxFormModule,
  DxTextBoxModule,
  DxSelectBoxModule,
  DxCheckBoxModule,
  DxNumberBoxModule,
  DxDateBoxModule,
  DxTextAreaModule,
  DxTagBoxModule,
  DxPopupModule,
  DxToolbarModule,
  DxTabPanelModule,
  DxScrollViewModule,
  DxLoadPanelModule,
  DxLoadIndicatorModule,
  DxValidatorModule,
  DxValidationSummaryModule,
  DxValidationGroupModule,
  DxTooltipModule,
  DxContextMenuModule,
  DxMenuModule,
  DxTreeViewModule,
  DxDrawerModule,
  DxListModule,
  DxDropDownButtonModule,
  DxSwitchModule,
  DxRadioGroupModule,
  DxLookupModule,
  DxFileUploaderModule,
  DxProgressBarModule,
  DxTemplateModule,
  DxBoxModule
} from 'devextreme-angular';

const DX_MODULES = [
  DxBoxModule,
  DxButtonModule,
  DxDataGridModule,
  DxFormModule,
  DxTextBoxModule,
  DxSelectBoxModule,
  DxCheckBoxModule,
  DxNumberBoxModule,
  DxDateBoxModule,
  DxTextAreaModule,
  DxTagBoxModule,
  DxPopupModule,
  DxToolbarModule,
  DxTabPanelModule,
  DxScrollViewModule,
  DxLoadPanelModule,
  DxLoadIndicatorModule,
  DxValidatorModule,
  DxValidationSummaryModule,
  DxValidationGroupModule,
  DxTooltipModule,
  DxContextMenuModule,
  DxMenuModule,
  DxTreeViewModule,
  DxDrawerModule,
  DxListModule,
  DxDropDownButtonModule,
  DxSwitchModule,
  DxRadioGroupModule,
  DxLookupModule,
  DxFileUploaderModule,
  DxProgressBarModule,
  DxTemplateModule
];

@NgModule({
  imports: [
    CommonModule,
    ...DX_MODULES
  ],
  exports: [
    CommonModule,
    ...DX_MODULES
  ]
})
export class SharedModule { }
