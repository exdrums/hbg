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
  DxChatModule
} from 'devextreme-angular';

// Routes
import { constructorRoutes } from './constructor.routes';

// Components
import { ConstructorMainComponent } from './components/constructor-main/constructor-main.component';
import { ProjectListComponent } from './components/project-list/project-list.component';
import { ConfigurationFormComponent } from './components/configuration-form/configuration-form.component';
import { ImageGalleryComponent } from './components/image-gallery/image-gallery.component';
import { ChatAssistantComponent } from './components/chat-assistant/chat-assistant.component';

// Services
import { ConstructorService } from './services/constructor.service';
import { ConstructorHubService } from './services/constructor-hub.service';

@NgModule({
  declarations: [
    ConstructorMainComponent,
    ProjectListComponent,
    ConfigurationFormComponent,
    ImageGalleryComponent,
    ChatAssistantComponent
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
    DxChatModule
  ],
  providers: [
    ConstructorService,
    ConstructorHubService
  ]
})
export class ConstructorModule { }
