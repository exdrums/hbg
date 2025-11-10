import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GrantsListComponent } from './grants-list/grants-list.component';

const routes: Routes = [
  {
    path: '',
    component: GrantsListComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GrantsRoutingModule { }
