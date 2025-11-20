import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

// DevExtreme
import {
  DxFormModule,
  DxButtonModule,
  DxSelectBoxModule,
  DxTextAreaModule,
  DxTabPanelModule,
  DxGalleryModule,
  DxLoadPanelModule,
  DxPopupModule,
  DxChatModule,
  DxSplitterModule,
  DxSpeedDialActionModule
} from 'devextreme-angular';

// Routes
import { constructorRoutes } from './constructor.routes';

// Components
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ConstructorWorkspaceComponent } from './components/constructor-workspace/constructor-workspace.component';
import { ConstructorViewComponent } from './components/constructor-view/constructor-view.component';
import { ConstructorConfigComponent } from './components/constructor-config/constructor-config.component';
import { ConstructorFormComponent } from './components/constructor-form/constructor-form.component';
import { ConstructorChatComponent } from './components/constructor-chat/constructor-chat.component';

// Services
import { ConstructorService } from './services/constructor.service';
import { ConstructorHubService } from './services/constructor-hub.service';

// Data Sources
import { ProjectsDataSource } from './data/projects.data-source';
import { ImagesDataSource } from './data/images.data-source';
import { ChatDataSource } from './data/chat.data-source';

@NgModule({
  declarations: [
    ProjectListComponent,
    ConstructorWorkspaceComponent,
    ConstructorViewComponent,
    ConstructorConfigComponent,
    ConstructorFormComponent,
    ConstructorChatComponent
  ],
  imports: [
    CommonModule,
    HttpClientModule,
    RouterModule.forChild(constructorRoutes),
    DxFormModule,
    DxButtonModule,
    DxSelectBoxModule,
    DxTextAreaModule,
    DxTabPanelModule,
    DxGalleryModule,
    DxLoadPanelModule,
    DxPopupModule,
    DxChatModule,
    DxSplitterModule,
    DxSpeedDialActionModule
  ],
  providers: [
    ConstructorService,
    ConstructorHubService,
    ProjectsDataSource,
    ImagesDataSource,
    ChatDataSource
  ]
})
export class ConstructorModule { }
