import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DxButtonModule, DxPopupModule, DxTooltipModule, DxLoadIndicatorModule, DxChatModule, DxTagBoxModule } from 'devextreme-angular';
import { HbgChatComponent } from './chat/chat.component';
import { ConversationsComponent } from './conversations/conversations.component';
import { ChatContainerComponent } from './chat-container/chat-container.component';
import { ChatService } from './chat.service';
import { ChatSignalRConnection } from './chat-connection.service';
import { ChatMessageStore } from './messages.data-store';
import { ChatConversationStore } from './conversations.data-store';

@NgModule({
  declarations: [
    HbgChatComponent,
    ConversationsComponent,
    ChatContainerComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    DxChatModule,
    DxButtonModule,
    DxPopupModule,
    DxTooltipModule,
    DxLoadIndicatorModule,
    DxTagBoxModule
  ],
  exports: [
    HbgChatComponent,
    ConversationsComponent,
    ChatContainerComponent
  ],
  providers: [
    ChatService,
    ChatSignalRConnection,
    ChatMessageStore,
    ChatConversationStore
  ]
})
export class ChatModule { }