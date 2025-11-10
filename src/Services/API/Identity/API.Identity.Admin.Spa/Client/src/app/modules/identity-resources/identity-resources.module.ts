import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { IdentityResourcesRoutingModule } from './identity-resources-routing.module';
import { IdentityResourcesListComponent } from './identity-resources-list/identity-resources-list.component';
import { IdentityResourceFormComponent } from './identity-resource-form/identity-resource-form.component';

@NgModule({
  declarations: [
    IdentityResourcesListComponent,
    IdentityResourceFormComponent
  ],
  imports: [
    SharedModule,
    IdentityResourcesRoutingModule
  ]
})
export class IdentityResourcesModule { }
