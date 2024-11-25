import { Component, NgModule } from '@angular/core';
import { ThemeService } from '@app/core/services/theme.service';
import { DxButtonModule } from 'devextreme-angular/ui/button';
import { ButtonStyle } from 'devextreme/common';

@Component({
  selector: 'hbg-oauth-buttons',
  templateUrl: './oauth-buttons.component.html',
  styleUrls: ['./oauth-buttons.component.scss']
})
export class OauthButtonsComponent {
  btnStylingMode: ButtonStyle;

  constructor(private themeService: ThemeService) {
    this.themeService.isDark.subscribe((value: boolean) => {
      this.btnStylingMode = value ? 'outlined' : 'contained';
    });
  }


}
