import { inject, Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { Plan } from "../models/plan.model";
import { ProjectsWebSocketConntection } from "./projects-ws-connection.service";
import { SignalRDataStore } from "@app/core/data/signalr-data-store";
import { CutImagePopupData } from "@app/core/components/image-cutter/cut-image-popup/cut-image-popup.component";
import { latLng } from "leaflet";
import { CoordinatesService } from "../planner/services/coordinates.service";

@Injectable()
export class PlansWsDataSource extends CustomDataSource<Plan> {
  private readonly crdnts = inject(CoordinatesService);
  constructor(private connection: ProjectsWebSocketConntection) {
    super(new SignalRDataStore<Plan>(connection, "planID", "Plan"));
  }

  public set projectId(value: number) {
    if (this.customStore.subjectId === value) return;
    this.customStore.subjectId = value;
    void this.reload();
  }


  public async refresh() {
    const plan = this.selected();
    await this.reload();
    if (!plan) return;
    const toSelect = plan ? this.items().find(x => x.planID === plan.planID) : this.items()[0] ?? undefined;
    this.setSelected(toSelect);
  }

  public async patchPositionCreated(data: CutImagePopupData, hasPicture: boolean) {
    const model: Plan = this.selected();
    const defaultPoint = latLng(46.879966, -121.676909);

    model.picCenterX = defaultPoint.lat;
    model.picCenterY = defaultPoint.lng;
    model.picWidth = data.resultWidth;
    model.picHeight = data.resultHeight;
    model.picScale = this.crdnts.defaultPlaneScaleValue;
    model.picRotation = 0;
    model.hasPlanPicture = hasPicture;
    
    await this.store().update(model.planID, model);
  }

  public async patchPosition(positions: { centerX: number, centerY: number, scale: number }) {
    const model: Plan = this.selected();

    model.picCenterX = positions.centerX;
    model.picCenterY = positions.centerY;
    model.picScale = positions.scale;

    await this.store().update(model.planID, model);
  }

}
