import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DxButtonModule, DxPopupModule, DxTooltipModule, DxLoadIndicatorModule, DxChatModule, DxTagBoxModule, DxTabPanelModule, DxDataGridModule, DxFormModule, DxContextMenuModule } from 'devextreme-angular';
import { MainChatComponent } from './components/main-chat/main-chat.component';
import { AlertService } from './services/alert.service';
import { AgentWebSocketConnection } from './connections/agent-ws.connection.service';
import { ChatWebSocketConnection } from './connections/chat-ws.connection.service';
import { EnhancedConversationsDataSourceService } from './data/enhanced-conversations.data-source';
import { MessagesDataSourceService } from './data/messages.data-source';
import { ConversationListComponent } from './components/conversation-list/conversation-list.component';
import { ChatViewComponent } from './components/chat-view/chat-view.component';

@NgModule({
  declarations: [
    MainChatComponent,
    ConversationListComponent,
    ChatViewComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    DxChatModule,
    DxContextMenuModule,
    DxButtonModule,
    DxPopupModule,
    DxTooltipModule,
    DxLoadIndicatorModule,
    DxTagBoxModule,
    DxTabPanelModule,
    DxDataGridModule,
    DxFormModule,
  ],
  exports: [
    // HbgChatComponent,
    // ConversationsComponent,
    // ChatContainerComponent,
    // ChatPageComponent,

    MainChatComponent,
    ConversationListComponent,
    ChatViewComponent
  ],
  providers: [
    AlertService,
    AgentWebSocketConnection,
    ChatWebSocketConnection,
    EnhancedConversationsDataSourceService,
    MessagesDataSourceService
  ]
})
export class ChatModule { }