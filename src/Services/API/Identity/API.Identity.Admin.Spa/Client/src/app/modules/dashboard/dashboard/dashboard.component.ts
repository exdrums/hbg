import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '@app/core/services/config.service';
import { firstValueFrom } from 'rxjs';

interface DashboardStats {
  clientsCount: number;
  usersCount: number;
  rolesCount: number;
  apiResourcesCount: number;
  identityResourcesCount: number;
  apiScopesCount: number;
  grantsCount: number;
}

@Component({
  selector: 'hbg-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  stats = [
    {
      title: 'Total Clients',
      value: 0,
      icon: 'globe',
      color: '#3498db',
      link: '/clients'
    },
    {
      title: 'Total Users',
      value: 0,
      icon: 'user',
      color: '#2ecc71',
      link: '/users'
    },
    {
      title: 'Total Roles',
      value: 0,
      icon: 'group',
      color: '#e74c3c',
      link: '/roles'
    },
    {
      title: 'API Resources',
      value: 0,
      icon: 'box',
      color: '#f39c12',
      link: '/api-resources'
    },
    {
      title: 'Identity Resources',
      value: 0,
      icon: 'card',
      color: '#9b59b6',
      link: '/identity-resources'
    },
    {
      title: 'API Scopes',
      value: 0,
      icon: 'tags',
      color: '#1abc9c',
      link: '/api-scopes'
    },
    {
      title: 'Active Grants',
      value: 0,
      icon: 'check',
      color: '#34495e',
      link: '/grants'
    }
  ];

  isLoading = true;
  recentActivity: any[] = [];

  constructor(
    private http: HttpClient,
    private config: ConfigService
  ) {}

  async ngOnInit() {
    await this.loadStats();
  }

  async loadStats() {
    try {
      this.isLoading = true;

      // Fetch counts from each endpoint in parallel
      const [clients, users, roles, apiResources, identityResources, apiScopes, grants] = await Promise.all([
        this.fetchCount('clients'),
        this.fetchCount('users'),
        this.fetchCount('roles'),
        this.fetchCount('apiresources'),
        this.fetchCount('identityresources'),
        this.fetchCount('apiscopes'),
        this.fetchCount('grants')
      ]);

      this.stats[0].value = clients;
      this.stats[1].value = users;
      this.stats[2].value = roles;
      this.stats[3].value = apiResources;
      this.stats[4].value = identityResources;
      this.stats[5].value = apiScopes;
      this.stats[6].value = grants;
    } catch (error) {
      console.error('Error loading dashboard statistics:', error);
      // Keep values at 0 on error
    } finally {
      this.isLoading = false;
    }
  }

  private async fetchCount(endpoint: string): Promise<number> {
    try {
      const baseUrl = this.config.hbgidentityadminapi;
      const url = `${baseUrl}/api/${endpoint}`;

      // Fetch the data array and return its length
      const response = await firstValueFrom(
        this.http.get<any[]>(url)
      );

      return Array.isArray(response) ? response.length : 0;
    } catch (error) {
      console.error(`Error fetching count for ${endpoint}:`, error);
      return 0;
    }
  }
}
