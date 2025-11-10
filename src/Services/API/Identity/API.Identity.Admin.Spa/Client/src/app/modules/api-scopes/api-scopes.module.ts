import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { ApiScopesRoutingModule } from './api-scopes-routing.module';
import { ApiScopesListComponent } from './api-scopes-list/api-scopes-list.component';
import { ApiScopeFormComponent } from './api-scope-form/api-scope-form.component';

@NgModule({
  declarations: [
    ApiScopesListComponent,
    ApiScopeFormComponent
  ],
  imports: [
    SharedModule,
    ApiScopesRoutingModule
  ]
})
export class ApiScopesModule { }
