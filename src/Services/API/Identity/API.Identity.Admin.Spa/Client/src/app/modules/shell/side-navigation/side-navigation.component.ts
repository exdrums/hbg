import { Component, Input, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'hbg-side-navigation',
  templateUrl: './side-navigation.component.html',
  styleUrls: ['./side-navigation.component.scss']
})
export class SideNavigationComponent implements OnInit {
  @Input() menuOpened: boolean = true;

  selectedItem: string = '';

  menuItems: any[] = [
    {
      text: 'Dashboard',
      path: '/dashboard',
      icon: 'home'
    },
    {
      text: 'OIDC Configuration',
      icon: 'key',
      items: [
        {
          text: 'Clients',
          path: '/clients',
          icon: 'globe'
        },
        {
          text: 'API Resources',
          path: '/api-resources',
          icon: 'box'
        },
        {
          text: 'API Scopes',
          path: '/api-scopes',
          icon: 'tags'
        },
        {
          text: 'Identity Resources',
          path: '/identity-resources',
          icon: 'card'
        }
      ]
    },
    {
      text: 'User Management',
      icon: 'group',
      items: [
        {
          text: 'Users',
          path: '/users',
          icon: 'user'
        },
        {
          text: 'Roles',
          path: '/roles',
          icon: 'group'
        }
      ]
    },
    {
      text: 'Grants',
      path: '/grants',
      icon: 'check'
    }
  ];

  constructor(private router: Router) {}

  ngOnInit() {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.selectedItem = this.router.url;
    });

    this.selectedItem = this.router.url;
  }

  onItemClick(event: any) {
    if (event.itemData.path) {
      this.router.navigate([event.itemData.path]);
    }
  }
}
