import { BehaviorSubject, Observable, Subject } from "rxjs";
import DxPopup, { dxPopupToolbarItem } from "devextreme/ui/popup";
import { Type } from "@angular/core";

/** All data requered for a Popup component, extend this for each IPopup */
export abstract class PopupContext<T extends IPopup<Data> = any, Data = any> {
constructor(
    public data: Data
) { }
    public readonly dxPopupComponent$ = new BehaviorSubject<DxPopup>(undefined);
    public readonly closed$: Subject<PopupResult<Data>> = new Subject<PopupResult<Data>>();
    public readonly clicked$: Subject<ToolbarID> = new Subject<ToolbarID>();


    public width: string = "auto";
    public height: string = "auto";
    public maxWidth: string;
    public maxHeight: string;
    public minWidth: string;
    public minHeight: string;
    public dragEnabled: boolean = true;
    public resizeEnabled: boolean = true;
    public showTitle: boolean = true;
    public title: string = "Default_title";
    public fullScreen: boolean = false;
    public abstract component: Type<T>;
    public abstract toolbar: PopupToolbarItem[];
    public popupInitialised(popup: DxPopup) {
        this.dxPopupComponent$.next(popup);
    }
    /**
     * Most commonly used buttons configuration
     */
    protected defaultOkCancelButtons: PopupToolbarItem[] = [
		{
			id: ToolbarID.Cancel,
			location: "after",
            widget: "dxButton",
			options: {
				text: "Cancel",
				onClick: () => this.clicked$.next(ToolbarID.Cancel),
			},
		},
		{
			id: ToolbarID.Ok,
			location: "after",
			widget: "dxButton",
			options: {
				text: "OK",
				onClick: () => this.closed$.next({ data: this.data, closedBy: ToolbarID.Ok }),
			},
		}
	];
}

/** Result Object returned after popup is closed */
export interface PopupResult<Data> {
    closedBy: ToolbarID,
    data: Data
}

/** Data object given by caller */
export interface IPopup<Data> {
    data$: Subject<PopupContext<IPopup<Data>, Data>>;
    data?: PopupContext<IPopup<Data>, Data>
}

export enum ToolbarID {
    Ok = 1,
    Cancel = 2
    // Other = 3 add other here
}

export interface PopupToolbarItem extends dxPopupToolbarItem {
    id: ToolbarID,
    disabled$?: Subject<boolean>,
    visible$?: Subject<boolean>
}
