import { inject, Injectable, OnDestroy } from '@angular/core';
import { Plan } from '../models/plan.model';
import { ConfigService } from '@app/core/services/config.service';
import { HttpClient } from '@angular/common/http';
import { map, of, tap } from 'rxjs';

interface BlobKey {
  id: number;
  blob: string;
}

@Injectable()
export class ImagesDataSource implements OnDestroy {
  private readonly config: ConfigService = inject(ConfigService);
  private readonly http: HttpClient = inject(HttpClient);
  constructor() { }
  private plansCache: BlobKey[] = [];
  private readonly url = (plan: Plan) => `${this.config.hbgfiles}/files/projects/${plan.projectID}/planimage/${plan.planID}`;
  private fetch$ = (plan: Plan) => this.http.get(this.url(plan), {responseType: "blob"});
  public post$(plan: Plan, file: File) {
    const url = this.url(plan);
    const formData = new FormData();
    formData.append("file", file);
    return this.http.post(url, formData);
  }

  public getPlanImage$(plan: Plan) {
    const cached = this.plansCache.find(p => p.id === plan.planID);
    if (cached) return of(cached.blob);

    return this.fetch$(plan).pipe(
      map(blob => URL.createObjectURL(blob)),
      tap(blobUrl => this.plansCache.push({ id: plan.planID, blob: blobUrl }))
    )
  }

  public removeFromCache(planID: number) {
    const cached = this.plansCache.find(b => b.id === planID);
    if (!cached) return;
    URL.revokeObjectURL(cached.blob);
    this.plansCache = this.plansCache.filter(c => c.id !== planID);
  }


  ngOnDestroy(): void {
    this.plansCache.forEach(c => URL.revokeObjectURL(c.blob));
  }
}
