import { inject, Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { Plan } from "../models/plan.model";
import { ProjectsWebSocketConntection } from "./projects-ws-connection.service";
import { SignalRDataStore } from "@app/core/data/signalr-data-store";
import { Article } from "../models/article.model";
import { PlansWsDataSource } from "./plans-ws.data-source";
import { from, map, Observable, switchMap } from "rxjs";

@Injectable()
export class ArticlesWsDataSource extends CustomDataSource<Article> {
  protected readonly plansDataSource = inject(PlansWsDataSource);
  constructor(private connection: ProjectsWebSocketConntection) {
    super(new SignalRDataStore<Article>(connection, "articleID", "Article"));
  }

  public set projectId(value: number) {
    if (this.customStore.subjectId === value) return;
    this.customStore.subjectId = value;
    void this.reload();
  }

  public readonly refresh = () => void this.reload();

  public readonly loadInSelectedPlan$: Observable<Article[]> = this.plansDataSource.selectedItem$.pipe(
    switchMap(plan => from(this.load()).pipe(
      // filter articles by planID
      map(loadedResult => loadedResult)
    ))
  );
}
