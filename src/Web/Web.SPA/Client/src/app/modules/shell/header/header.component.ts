import { Component, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '@app/core/services/auth.service';

@Component({
  selector: 'hbg-header',
  templateUrl: 'header.component.html',
  styleUrls: ['./header.component.scss'],
})

export class HeaderComponent {
  constructor(private authService: AuthService) { }
  @Output()
  menuToggle = new EventEmitter<boolean>();

  @Input()
  menuToggleEnabled = false;

  @Input()
  title!: string;

  user;  // user: IUser | null = { email: '' };

  userMenuItems = [
  {
    text: 'Logout',
    icon: 'runner',
    onClick: () => {
      this.authService.logout();
    },
  }];

  toggleMenu = () => {
    this.menuToggle.emit();
  };
}
