import { Component, OnInit } from '@angular/core';
import { ThemeService } from '@app/core/services/theme.service';

@Component({
  selector: 'hbg-theme-switcher',
  templateUrl: './theme-switcher.component.html',
  styleUrls: ['./theme-switcher.component.scss']
})
export class ThemeSwitcherComponent implements OnInit {
  isDark: boolean = false;

  constructor(private themeService: ThemeService) {}

  ngOnInit() {
    this.themeService.isDark.subscribe(isDark => {
      this.isDark = isDark;
    });
  }

  toggleTheme() {
    this.themeService.switchTheme();
  }
}
