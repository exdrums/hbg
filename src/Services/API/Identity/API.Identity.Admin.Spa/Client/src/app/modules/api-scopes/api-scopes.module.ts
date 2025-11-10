import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DxDataGridModule, DxButtonModule } from 'devextreme-angular';

import { ApiScopesListComponent } from './api-scopes-list/api-scopes-list.component';

@NgModule({
  declarations: [
    ApiScopesListComponent
  ],
  imports: [
    CommonModule,
    DxDataGridModule,
    DxButtonModule,
    RouterModule.forChild([
      {
        path: '',
        component: ApiScopesListComponent
      }
    ])
  ]
})
export class ApiScopesModule { }
