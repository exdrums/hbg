import { NgModule } from "@angular/core";
import {
  DxButtonModule,
  DxChartModule,
  DxColorBoxModule,
  DxDataGridModule,
  DxFileUploaderModule,
  DxFormModule,
  DxFunnelModule,
  DxMultiViewModule,
  DxPieChartModule,
  DxPolarChartModule,
  DxRangeSliderModule,
  DxResizableModule,
  DxSankeyModule,
  DxScrollViewModule,
  DxSpeedDialActionModule,
  DxTabPanelModule,
  DxTextAreaModule,
  DxToolbarModule,
  DxTreeMapModule,
} from "devextreme-angular";

@NgModule({
  imports: [
    DxButtonModule,
    DxMultiViewModule,
    DxToolbarModule,
    DxFormModule,
    DxResizableModule,

    DxChartModule,
    DxPieChartModule,
    DxPolarChartModule,
    DxTreeMapModule,
    DxFunnelModule,
    DxSankeyModule,

    DxDataGridModule,
    DxColorBoxModule,
    DxTabPanelModule,
    DxSpeedDialActionModule,
    DxScrollViewModule,
    DxTextAreaModule,
    DxRangeSliderModule,

    DxFileUploaderModule
  ],
  exports: [
    DxButtonModule,
    DxMultiViewModule,
    DxToolbarModule,
    DxFormModule,
    DxResizableModule,

    DxChartModule,
    DxPieChartModule,
    DxPolarChartModule,
    DxTreeMapModule,
    DxFunnelModule,
    DxSankeyModule,

    DxDataGridModule,
    DxColorBoxModule,
    DxTabPanelModule,
    DxSpeedDialActionModule,
    DxScrollViewModule,
    DxTextAreaModule,
    DxRangeSliderModule,

    DxFileUploaderModule
  ],
})
export class HbgDxModule {}
