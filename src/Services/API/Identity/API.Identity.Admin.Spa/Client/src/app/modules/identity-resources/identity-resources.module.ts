import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { DxDataGridModule, DxButtonModule } from 'devextreme-angular';

import { IdentityResourcesListComponent } from './identity-resources-list/identity-resources-list.component';

@NgModule({
  declarations: [
    IdentityResourcesListComponent
  ],
  imports: [
    CommonModule,
    DxDataGridModule,
    DxButtonModule,
    RouterModule.forChild([
      {
        path: '',
        component: IdentityResourcesListComponent
      }
    ])
  ]
})
export class IdentityResourcesModule { }
