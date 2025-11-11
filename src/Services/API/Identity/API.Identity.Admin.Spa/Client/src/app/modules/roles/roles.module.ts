import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { RolesRoutingModule } from './roles-routing.module';
import { RolesListComponent } from './roles-list/roles-list.component';
import { RoleFormComponent } from './role-form/role-form.component';

@NgModule({
  declarations: [
    RolesListComponent,
    RoleFormComponent
  ],
  imports: [
    SharedModule,
    RolesRoutingModule
  ]
})
export class RolesModule { }
