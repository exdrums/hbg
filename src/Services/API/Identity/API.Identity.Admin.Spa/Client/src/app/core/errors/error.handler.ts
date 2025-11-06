import { ErrorHandler, Injectable } from "@angular/core";
import { PopupService } from "../services/popup.service";
import { ErrorPopupContext, ErrorPopupData, PopupErrorComponent } from "../components/hbg-popup/popup-error/popup-error.component";

export interface FrontEndError {}

@Injectable()
export class FrontendErrorHandler implements ErrorHandler {
  constructor(private readonly popup: PopupService) { }

  public handleError(error: Error) {
    console.error(error);
    const data: ErrorPopupData = { clientError: error };
    this.popup.pushPopup<PopupErrorComponent, ErrorPopupData>(new ErrorPopupContext(data)).subscribe();
  }
}

export const FrontEndErrorHandlerProvider = {
  provide: ErrorHandler,
  useClass: FrontendErrorHandler
}
