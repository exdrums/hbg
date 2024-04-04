import { Component } from '@angular/core';
import { applyDevExtremeDefaults } from './core/devextreme/default-settings';
import { MessagesHubService } from './core/services/websocket/messages-hub.service';
import { PopupService } from './core/services/popup.service';
import { ScreenLockerService } from './core/services/screen-locker.service';
import dxChart from 'devextreme/viz/chart';

@Component({
	selector: 'hbg-root',
	templateUrl: './app.component.html',
	styleUrls: ['./app.component.scss']
})
export class AppComponent {
constructor( 
    private readonly popupService: PopupService, 
    private readonly hub: MessagesHubService, 
    public readonly lock: ScreenLockerService) {
        applyDevExtremeDefaults(); 
        // const data: ExamplePopupData = { someInputData: "some input text"};
        // this.popupService.pushPopup<ExamplePopupComponent, ExamplePopupData>(new ExamplePopupContext(data)).subscribe(x => console.log('popup_result:', x));
    }
    private x : dxChart
}
