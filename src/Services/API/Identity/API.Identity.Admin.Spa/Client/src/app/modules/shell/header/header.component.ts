import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { AuthService } from '@app/core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'hbg-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  @Output() menuToggle = new EventEmitter<void>();
  @Input() menuToggleEnabled = false;

  userMenuItems: any[] = [
    {
      text: 'Profile',
      icon: 'user',
      onClick: () => this.onProfileClick()
    },
    {
      text: 'Logout',
      icon: 'runner',
      onClick: () => this.onLogoutClick()
    }
  ];

  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  toggleMenu = () => {
    this.menuToggle.emit();
  };

  onProfileClick() {
    // Navigate to profile page (to be implemented)
    console.log('Profile clicked');
  }

  onLogoutClick() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  userName = 'User';

  ngOnInit() {
    this.authService.userProfile$.subscribe(profile => {
      this.userName = profile?.name || profile?.email || 'User';
    });
  }
}
