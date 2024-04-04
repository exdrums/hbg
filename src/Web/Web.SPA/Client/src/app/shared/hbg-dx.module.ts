import { NgModule } from "@angular/core";
import {
  DxButtonModule,
  DxChartModule,
  DxColorBoxModule,
  DxDataGridModule,
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
    DxRangeSliderModule
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
    DxRangeSliderModule
  ],
})
export class HbgDxModule {}
