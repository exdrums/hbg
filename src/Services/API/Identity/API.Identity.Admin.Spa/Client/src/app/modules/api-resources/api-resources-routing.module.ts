import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ApiResourcesListComponent } from './api-resources-list/api-resources-list.component';
import { ApiResourceFormComponent } from './api-resource-form/api-resource-form.component';

const routes: Routes = [
  {
    path: '',
    component: ApiResourcesListComponent
  },
  {
    path: 'new',
    component: ApiResourceFormComponent
  },
  {
    path: ':id',
    component: ApiResourceFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ApiResourcesRoutingModule { }
