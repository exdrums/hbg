import { Component } from '@angular/core';
import { applyDevExtremeDefaults } from './core/devextreme/default-settings';
import { PopupService } from './core/services/popup.service';
import { ScreenLockerService } from './core/services/screen-locker.service';

@Component({
  selector: 'hbg-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  constructor(
    private readonly popupService: PopupService,
    public readonly lock: ScreenLockerService
  ) {
    applyDevExtremeDefaults();
  }
}
