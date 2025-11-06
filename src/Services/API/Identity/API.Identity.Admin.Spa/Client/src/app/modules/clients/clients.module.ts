import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { ClientsRoutingModule } from './clients-routing.module';
import { ClientsListComponent } from './clients-list/clients-list.component';

@NgModule({
  declarations: [
    ClientsListComponent
  ],
  imports: [
    SharedModule,
    ClientsRoutingModule
  ]
})
export class ClientsModule { }
