import { Injectable } from "@angular/core";
import { CustomDataSource } from "@app/core/data/custom-data-source";
import { Plan } from "../models/plan.model";
import { ProjectsWebSocketConntection } from "./projects-ws-connection.service";
import { SignalRDataStore } from "@app/core/data/signalr-data-store";
import { Article } from "../models/article.model";

@Injectable()
export class ArticlesWsDataSource extends CustomDataSource<Article> {
  constructor(private connection: ProjectsWebSocketConntection) {
    super(new SignalRDataStore<Article>(connection, "articleID", "Article"));
  }

  public set projectId(value: number) {
    if (this.customStore.subjectId === value) return;
    this.customStore.subjectId = value;
    void this.reload();
  }

  public readonly refresh = () => void this.reload();
}
