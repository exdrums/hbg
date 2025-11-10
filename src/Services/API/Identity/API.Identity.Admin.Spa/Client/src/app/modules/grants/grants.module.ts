import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { GrantsRoutingModule } from './grants-routing.module';
import { GrantsListComponent } from './grants-list/grants-list.component';

@NgModule({
  declarations: [
    GrantsListComponent
  ],
  imports: [
    SharedModule,
    GrantsRoutingModule
  ]
})
export class GrantsModule { }
