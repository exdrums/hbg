import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { ApiResourcesRoutingModule } from './api-resources-routing.module';
import { ApiResourcesListComponent } from './api-resources-list/api-resources-list.component';
import { ApiResourceFormComponent } from './api-resource-form/api-resource-form.component';

@NgModule({
  declarations: [
    ApiResourcesListComponent,
    ApiResourceFormComponent
  ],
  imports: [
    SharedModule,
    ApiResourcesRoutingModule
  ]
})
export class ApiResourcesModule { }
