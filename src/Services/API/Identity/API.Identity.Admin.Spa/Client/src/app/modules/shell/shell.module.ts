import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { RouterModule } from '@angular/router';

import { ShellComponent } from './shell/shell.component';
import { HeaderComponent } from './header/header.component';
import { SideNavigationComponent } from './side-navigation/side-navigation.component';
import { ThemeSwitcherComponent } from './theme-switcher/theme-switcher.component';

@NgModule({
  declarations: [
    ShellComponent,
    HeaderComponent,
    SideNavigationComponent,
    ThemeSwitcherComponent
  ],
  imports: [
    SharedModule,
    RouterModule
  ],
  exports: [
    ShellComponent
  ]
})
export class ShellModule { }
