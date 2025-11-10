import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ApiScopesListComponent } from './api-scopes-list/api-scopes-list.component';
import { ApiScopeFormComponent } from './api-scope-form/api-scope-form.component';

const routes: Routes = [
  {
    path: '',
    component: ApiScopesListComponent
  },
  {
    path: 'new',
    component: ApiScopeFormComponent
  },
  {
    path: ':id',
    component: ApiScopeFormComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ApiScopesRoutingModule { }
