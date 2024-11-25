import { Component, ViewChild } from '@angular/core';
import { SafeUrl } from '@angular/platform-browser';
import { IPopup, PopupContext, PopupToolbarItem, ToolbarID } from '@app/core/components/hbg-popup/popup-context';
import { BehaviorSubject, Observable, Subject, filter, of, switchMap, take, tap } from 'rxjs';
import { ImageCutterComponent } from '../image-cutter.component';

export interface ResultImageSettings {
  bw: boolean;
  quality: number;
  scaleFactor: number;
  isMono: boolean;
}

export interface CutImagePopupData {
	url: SafeUrl;
	blob?: Blob | File;
  resultWidth?: number;
  resultHeight?: number;
}

export class CutImagePopupContext extends PopupContext<CutImagePopupComponent, CutImagePopupData> {
  public fullScreen: boolean = true;
  public component = CutImagePopupComponent;
  public saveChangesClicked$ = new Subject();
  public title = "Cut image";
  public toolbar: PopupToolbarItem[] = [
    {
      id: ToolbarID.Cancel,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Cancel",
        type: "normal",
        width: 120,
        onClick: () => {
          console.log('CancelClicked');
          this.closed$.next({ data: this.data, closedBy: ToolbarID.Cancel });
        }
      }
    }, {
      id: ToolbarID.Ok,
      location: "after",
      widget: "dxButton",
      options: {
        text: "Save",
        type: "success",
        width: 120,
        onClick: () => {
          console.log('OkClicked');
          this.saveChangesClicked$.next(null);
        }
      }
    }
  ];
}

@Component({
  selector: 'hbg-cut-image-popup',
  templateUrl: './cut-image-popup.component.html',
  styleUrls: ['./cut-image-popup.component.scss']
})
export class CutImagePopupComponent implements IPopup<CutImagePopupData> {
  constructor() {
    this.saving$().subscribe();
  }

  public readonly data$ = new BehaviorSubject<CutImagePopupContext>(undefined);

  //#region Configurations

	@ViewChild("cutter") public cutter: ImageCutterComponent;
	public config: Cropper.Options = {
		viewMode: 0,
		dragMode: "move",
		zoomOnWheel: true,
		zoomable: true,
		scalable: true,
		movable: true,
		aspectRatio: NaN,
		autoCropArea: 0.9,
		background: true,
		ready: () => {
			this.cutter.cropper.zoom(-0.11111111);
			this.getAdjustedCanvas();
		},
  };
  private pixelsUp = 11000000;
	private pixelsTo = 9000000;
	private maxAllowedFileSize = 1024 * 1024;
  public imageSettings: ResultImageSettings = {
    bw: false,
    quality: 0.9,
    scaleFactor: 1,
    isMono: false
  };

  //#endregion

  private saving$ = () => this.data$.pipe(
    filter(c => c != null),
    switchMap(data => data.saveChangesClicked$.pipe(
      take(1),
      tap(() => this.exportAndClose(data))
    ))
  )

  private exportAndClose(context: CutImagePopupContext) {
		// hide popup for loading
		// context.dxPopupComponent$.value.option("visible", false);
		this.exportBlob$().subscribe((result) => {
			context.data.url = null;
      context.data.blob = result.blob;
      context.data.resultWidth = result.canvas.width;
      context.data.resultHeight = result.canvas.height;
      context.closed$.next({ data: context.data, closedBy: ToolbarID.Ok });
		});
  }
  
  private exportBlob$(): Observable<{ blob: Blob , canvas: HTMLCanvasElement }> {
		// get adjusted canvas to max9-11 megapixel
    const canvas = this.getAdjustedCanvas();
		// get first cropped BLOB (without any resolution/quality changes)
		return this.adjustQuality$(canvas);
  }
  
  private readonly adjustQuality$ = (canvas: HTMLCanvasElement): Observable<{ blob: Blob , canvas: HTMLCanvasElement }> => this.getBlob$(canvas).pipe(
    switchMap((blob) => {
      if (blob.size > this.maxAllowedFileSize) {
        let step = 0.1;
        if (this.imageSettings.quality < 0.5) step = 0.05;
        if (this.imageSettings.quality < 0.1) step = 0.01;
        this.imageSettings.quality = this.imageSettings.quality - step;
        blob = null;
        return this.adjustQuality$(canvas);
      }
      else return of({ blob, canvas});
    })
  );
  
  /**
	 * Returns edited Blob from Canvas
	 */
	private readonly getBlob$ = (canvas: HTMLCanvasElement) =>
		new Observable<Blob>((s) => {
			canvas.toBlob(
				(b) => {
					s.next(b);
					s.complete();
				},
				"image/jpeg",
				this.imageSettings.quality
			);
		});

	private getAdjustedCanvas(): HTMLCanvasElement {
		const data = this.cutter.cropper?.getData(true) || { width: 100, height: 100 };
		const croppedPixels = data.width * data.height;

		if (croppedPixels > this.pixelsUp) {
			this.imageSettings.scaleFactor = Math.pow(this.pixelsTo / croppedPixels, 0.5);
			this.imageSettings.quality = 0.7;
		}

		const options = {
			fillColor: "#fff",
			width: data.width * this.imageSettings.scaleFactor,
			height: data.height * this.imageSettings.scaleFactor,
			imageSmoothingEnabled: false,
		};
		const canvas = this.cutter.cropper.getCroppedCanvas(options);
		// apply BW setting
		this.applyMonoData(canvas);
		return canvas;
  }
  
	private applyMonoData(canvas: HTMLCanvasElement) {
		if (!this.imageSettings.isMono) return;

		const ctx = canvas.getContext("2d");
		const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
		for (let y = 0; y < imageData.height; y++) {
			for (let x = 0; x < imageData.width; x++) {
				const i = y * 4 * imageData.width + x * 4;
				const avg = (imageData.data[i] + imageData.data[i + 1] + imageData.data[i + 2]) / 3;
				imageData.data[i] = avg;
				imageData.data[i + 1] = avg;
				imageData.data[i + 2] = avg;
			}
		}
		ctx.putImageData(imageData, 0, 0, 0, 0, imageData.width, imageData.height);
	}
}
