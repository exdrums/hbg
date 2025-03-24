import { Component, inject, OnInit } from '@angular/core';
import { ArticlesWsDataSource } from '@app/modules/projects/data/articles-ws.data-source';
import { PlansWsDataSource } from '@app/modules/projects/data/plans-ws.data-source';
import { BaseLayerComponent } from '../base-layer';
import { icon, LayerGroup, layerGroup, marker } from 'leaflet';
import { firstValueFrom, from, map, switchMap } from 'rxjs';
import { Article } from '@app/modules/projects/models/article.model';

@Component({
  selector: 'hbg-article-icons-layer',
  templateUrl: './article-icons-layer.component.html',
  styleUrls: ['./article-icons-layer.component.scss']
})
export class ArticleIconsLayerComponent extends BaseLayerComponent implements OnInit {
  constructor() { super(); }

  public layer: LayerGroup = layerGroup();

  public layer$ = this.articlesDataSource.loadInSelectedPlan$.pipe(
    map(articles => this.createArticleMarkers)
  );

  private createArticleMarkers(articles: Article[]) {
    this.layer.clearLayers();

    for (const article of articles) {
      const articleIcon = icon({
        iconUrl: `assets/article-icons/${article.typeAsString}.png`, // Path to device icon
        iconSize: [40, 40], // Adjust as needed
      });

      const articleMarker = marker([article.y, article.x], { 
        icon: articleIcon, 
        draggable: true 
      }).addTo(this.layer);

      articleMarker.on('dragend', (event) => this.onMarkerDragEnd(event, article));
    }
  }

  onMarkerDragEnd(event: L.LeafletEvent, device: any): void {
    const marker = event.target as L.Marker;
    const position = marker.getLatLng();

    //
    // this.articlesDataSource.store().update();

    // Update device position in the backend
    // this.http.put(`/api/devices/${device.id}`, {
    //   x: position.lng,
    //   y: position.lat
    // }).subscribe(() => {
    //   console.log(`Device ${device.id} position updated.`);
    // });
  }


  ngOnInit() {
  }

}
