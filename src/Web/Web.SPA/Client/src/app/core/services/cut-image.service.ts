import { Injectable, inject } from "@angular/core";
import { PopupService } from "./popup.service";
import { DomSanitizer } from "@angular/platform-browser";
import { Observable, filter, finalize, from, map, tap } from "rxjs";
import { CutImagePopupComponent, CutImagePopupContext, CutImagePopupData } from "@app/core/components/image-cutter/cut-image-popup/cut-image-popup.component";
import { ToolbarID } from "../components/hbg-popup/popup-context";

@Injectable()
export class CutImageService {
	constructor() {}
    private readonly popups = inject(PopupService);
    private readonly domSanitizer = inject(DomSanitizer);

	public cutImage(file: File): Observable<CutImagePopupData> {
		const url = URL.createObjectURL(file);
		return this.popups.pushPopup<CutImagePopupComponent, CutImagePopupData>(new CutImagePopupContext({ url: this.domSanitizer.bypassSecurityTrustResourceUrl(url) }))
            .pipe(
				filter((x) => x.data.blob !== null),
				map((result) =>
                    result.closedBy === ToolbarID.Ok ?
						{ ...result.data, blob: new File([result.data.blob], "hbg-image.jpg", { lastModified: file.lastModified, type: result.data.blob.type }) }
                        : null
				),
				finalize(() => URL.revokeObjectURL(url))
			);
	}

	// public readonly selectImageAndCropp = (): Observable<File> => from(openImageFile()).pipe(
	// 	filter((file) => file != null),
	// 	switchMap((file) => this.croppImage(file)));
}
