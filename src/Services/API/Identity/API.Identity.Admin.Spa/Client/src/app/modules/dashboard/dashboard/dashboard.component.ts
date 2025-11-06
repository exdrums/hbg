import { Component, OnInit } from '@angular/core';

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
    }
  ];

  recentActivity: any[] = [];

  constructor() {}

  ngOnInit() {
    // TODO: Load actual statistics from API
    // For now, using placeholder data
    this.loadStats();
  }

  loadStats() {
    // Placeholder - would fetch from API in real implementation
    this.stats[0].value = 5;  // Clients
    this.stats[1].value = 12; // Users
    this.stats[2].value = 4;  // Roles
    this.stats[3].value = 8;  // API Resources
  }
}
