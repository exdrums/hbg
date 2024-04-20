import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import Cropper from 'cropperjs';

export interface ImageCutterSetting {
  width: number;
  height: number;
}

export interface ImageCutterResult {
  imageData: Cropper.ImageData;
  cropData: Cropper.CropBoxData;
  blob?: Blob;
  dataUrl?: string;
}

@Component({
  selector: 'hbg-image-cutter',
  templateUrl: './image-cutter.component.html',
  styleUrls: ['./image-cutter.component.scss']
})
export class ImageCutterComponent {
  constructor() { }
  @ViewChild('image') image: ElementRef;
  @Input() imageUrl: any;
  @Input() settings: ImageCutterSetting;
  @Input() cropbox: Cropper.CropBoxData;
  @Input() loadImageErrorText: string;
  @Input() cropperOptions: any = {};
  @Output() export = new EventEmitter<ImageCutterResult>();
  @Output() ready = new EventEmitter();

  public isLoading = true;
  public cropper: Cropper;
  public imageElement: HTMLImageElement;
  public loadError: any;

  public onImageLoaded(ev: Event) {
    this.loadError = false;

    const image = ev.target as HTMLImageElement;
    this.imageElement = image;

    if (this.cropperOptions.checkCrossOrigin) image.crossOrigin = 'anonymous';

    image.addEventListener('ready', () => {
      this.ready.emit(true);
      this.isLoading = false;
      if (this.cropbox) {
        this.cropper.setCropBoxData(this.cropbox);
      }
    });

    let aspectRatio = NaN;
    if (this.settings) {
      const { width, height } = this.settings;
      aspectRatio = width / height;
    }

    this.cropperOptions = Object.assign({
      aspectRatio,
      movable: false,
      scalable: false,
      zoomable: false,
      viewMode: 1,
      checkCrossOrigin: true
    }, this.cropperOptions);

    if (this.cropper) {
      this.cropper.destroy();
      this.cropper = undefined;
    }
    this.cropper = new Cropper(image, this.cropperOptions);
  }


  public onImageLoadError(event: any) {
    this.loadError = true;
    this.isLoading = false;
  }
}
