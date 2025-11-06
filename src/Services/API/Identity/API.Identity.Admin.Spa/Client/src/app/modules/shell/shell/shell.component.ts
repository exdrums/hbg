import { Component, OnInit } from '@angular/core';
import { AuthService } from '@app/core/services/auth.service';
import { ScreenService } from '@app/core/services/screen.service';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'hbg-shell',
  templateUrl: './shell.component.html',
  styleUrls: ['./shell.component.scss']
})
export class ShellComponent implements OnInit {
  menuOpened: boolean = true;
  menuMode: 'overlap' | 'shrink' | 'push' = 'shrink';
  menuRevealMode: 'slide' | 'expand' = 'expand';
  minMenuSize: number = 60;
  shaderEnabled: boolean = false;

  constructor(
    public authService: AuthService,
    private screen: ScreenService,
    private router: Router
  ) {}

  ngOnInit() {
    this.updateDrawer();

    this.screen.screenChanged.subscribe(() => this.updateDrawer());

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      if (this.menuMode === 'overlap') {
        this.menuOpened = false;
      }
    });
  }

  updateDrawer() {
    const isXSmall = this.screen.sizes['screen-x-small'];
    const isLarge = this.screen.sizes['screen-large'];

    if (isLarge) {
      this.menuMode = 'shrink';
      this.menuRevealMode = 'expand';
      this.minMenuSize = 60;
      this.shaderEnabled = false;
    } else if (isXSmall) {
      this.menuMode = 'overlap';
      this.menuRevealMode = 'slide';
      this.minMenuSize = 0;
      this.shaderEnabled = true;
      this.menuOpened = false;
    } else {
      this.menuMode = 'overlap';
      this.menuRevealMode = 'slide';
      this.minMenuSize = 0;
      this.shaderEnabled = true;
    }
  }

  toggleMenu = () => {
    this.menuOpened = !this.menuOpened;
  };

  get hideMenuAfterNavigation() {
    return this.menuMode === 'overlap';
  }

  get showMenuAfterClick() {
    return !this.menuOpened;
  }
}
