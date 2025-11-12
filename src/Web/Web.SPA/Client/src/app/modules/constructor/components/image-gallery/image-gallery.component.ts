import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { GeneratedImage } from '../../models';

@Component({
  selector: 'app-image-gallery',
  templateUrl: './image-gallery.component.html',
  styleUrls: ['./image-gallery.component.scss']
})
export class ImageGalleryComponent implements OnChanges {
  @Input() images: GeneratedImage[] = [];
  @Input() loading: boolean = false;

  selectedIndex: number = 0;

  ngOnChanges(changes: SimpleChanges) {
    if (changes['images'] && this.images.length > 0) {
      // Select the latest image when new one is added
      this.selectedIndex = 0;
    }
  }

  downloadImage(image: GeneratedImage) {
    window.open(image.fileServiceUrl, '_blank');
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }
}
