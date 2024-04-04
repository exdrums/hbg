import { RouterModule } from "@angular/router";
import { ShellComponent } from "./shell.component";
import { CommonModule } from "@angular/common";
import { DxButtonModule, DxDrawerModule, DxDropDownButtonModule, DxListModule, DxToolbarModule, DxTreeViewModule } from "devextreme-angular";
import { NgModule } from "@angular/core";
import { SideNavigationMenuComponent } from "./side-navigation-menu/side-navigation-menu.component";
import { HeaderComponent } from "./header/header.component";
import { FooterComponent } from "./footer/footer.component";
import { ThemeSwitcherComponent } from "./theme-switcher/theme-switcher.component";
import { UserPanelComponent } from "./user-panel/user-panel.component";
import { UserMenuSectionComponent } from "./user-menu-section/user-menu-section.component";

@NgModule({
    imports: [
        RouterModule,
        DxDrawerModule,
        DxTreeViewModule,
        DxButtonModule,
        DxListModule,
        DxDropDownButtonModule,
        DxToolbarModule,
        CommonModule
    ],
    exports: [
        // ShellComponent
    ],
    declarations: [
        HeaderComponent,
        FooterComponent,
        SideNavigationMenuComponent,
        ShellComponent,
        ThemeSwitcherComponent,
        UserMenuSectionComponent,
        UserPanelComponent
    ],
  })
  export class ShellModule { }