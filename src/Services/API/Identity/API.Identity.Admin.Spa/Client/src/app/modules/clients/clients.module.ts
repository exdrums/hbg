import { NgModule } from '@angular/core';
import { SharedModule } from '@app/shared/shared.module';
import { ClientsRoutingModule } from './clients-routing.module';
import { ClientsListComponent } from './clients-list/clients-list.component';
import { ClientFormComponent } from './client-form/client-form.component';

@NgModule({
  declarations: [
    ClientsListComponent,
    ClientFormComponent
  ],
  imports: [
    SharedModule,
    ClientsRoutingModule
  ]
})
export class ClientsModule { }
