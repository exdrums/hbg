import { Component, Input, OnChanges, SimpleChanges, OnInit } from '@angular/core';
import { GeneratedImage } from '../../models';
import { ImagesDataSource } from '../../data/images.data-source';
import { ConfigService } from '@app/core/services/config.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-constructor-view',
  templateUrl: './constructor-view.component.html',
  styleUrls: ['./constructor-view.component.scss'],
  providers: [ImagesDataSource]
})
export class ConstructorViewComponent implements OnInit, OnChanges {
  @Input() images: GeneratedImage[] = [];
  @Input() loading: boolean = false;
  @Input() projectId: string | null = null;

  selectedIndex: number = 0;
  imagesDataSource: ImagesDataSource;

  constructor(
    config: ConfigService,
    http: HttpClient
  ) {
    this.imagesDataSource = new ImagesDataSource(config, http);
  }

  ngOnInit() {
    if (this.projectId) {
      this.imagesDataSource.setProjectId(this.projectId);
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['images'] && this.images.length > 0) {
      // Select the latest image when new one is added
      this.selectedIndex = 0;
    }

    if (changes['projectId'] && this.projectId) {
      this.imagesDataSource.setProjectId(this.projectId);
    }
  }

  downloadImage(image: GeneratedImage) {
    if (image.fileServiceUrl) {
      window.open(image.fileServiceUrl, '_blank');
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }

  getImageUrl(image: GeneratedImage): string {
    return image.fileServiceUrl || image.thumbnailUrl || '';
  }
}
