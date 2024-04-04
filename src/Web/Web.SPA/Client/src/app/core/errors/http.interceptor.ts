import { HTTP_INTERCEPTORS, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { catchError, of, throwError } from "rxjs";
import { PopupService } from "../services/popup.service";
import { ErrorPopupContext, ErrorPopupData, PopupErrorComponent } from "../components/hbg-popup/popup-error/popup-error.component";

@Injectable()
export class MainHttpInterceptor implements HttpInterceptor {
    constructor(private readonly popup: PopupService) { }
    
    public intercept(request: HttpRequest<unknown>, next: HttpHandler) {
        // console.log('Request prepared', request);
        return next.handle(request).pipe(
            catchError(error => {
                console.error('HttpError intercepted', error);
                const data: ErrorPopupData = { serverError: error };
                this.popup.pushPopup<PopupErrorComponent, ErrorPopupData>(new ErrorPopupContext(data)).subscribe();
                return of(null);
            })
        )
    }
}

export const MainHttpInterceptorProvider = {
    provide: HTTP_INTERCEPTORS,
    useClass: MainHttpInterceptor,
    multi: true
}