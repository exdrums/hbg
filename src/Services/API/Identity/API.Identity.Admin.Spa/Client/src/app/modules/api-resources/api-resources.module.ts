import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DxDataGridModule, DxButtonModule } from 'devextreme-angular';

import { ApiResourcesListComponent } from './api-resources-list/api-resources-list.component';

@NgModule({
  declarations: [
    ApiResourcesListComponent
  ],
  imports: [
    CommonModule,
    DxDataGridModule,
    DxButtonModule,
    RouterModule.forChild([
      {
        path: '',
        component: ApiResourcesListComponent
      }
    ])
  ]
})
export class ApiResourcesModule { }
