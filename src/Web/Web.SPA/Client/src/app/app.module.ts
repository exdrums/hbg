import { NgModule } from '@angular/core';
import { CoreModule } from './core/core.module';
import { RouterModule } from '@angular/router';
import { routes } from './app.routes';
import { ServiceWorkerModule } from '@angular/service-worker';

import { AuthModule } from './modules/auth/auth.module';
import { HomeModule } from './modules/home/home.module';
import { ShellModule } from './modules/shell/shell.module';
import { environment } from '../environments/environment';

import { AppComponent } from './app.component';
import { ProjectsModule } from './modules/projects/projects.module';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    CoreModule,
    RouterModule.forRoot(routes, { useHash: true }),
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production }),

    AuthModule,
    HomeModule,
    ProjectsModule,
    ShellModule,
  ],

  bootstrap: [AppComponent]
})
export class AppModule { }
