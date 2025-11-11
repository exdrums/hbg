import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IdentityResourcesListComponent } from './identity-resources-list/identity-resources-list.component';
import { IdentityResourceFormComponent } from './identity-resource-form/identity-resource-form.component';

const routes: Routes = [
  {
    path: '',
    component: IdentityResourcesListComponent
  },
  {
    path: 'new',
    component: IdentityResourceFormComponent
  },
  {
    path: ':id',
    component: IdentityResourceFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class IdentityResourcesRoutingModule { }
