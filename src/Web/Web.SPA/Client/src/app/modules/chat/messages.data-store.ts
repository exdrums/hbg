import { Injectable } from "@angular/core";
import { firstValueFrom } from "rxjs";
import { CustomStore } from "devextreme-angular/common/data";
import { ChatSignalRConnection } from "./chat-connection.service";
import { ConversationType } from "./models/conversation-type";
import { ChatMessageAdapter } from "./models/chat-message-adapter";
import { Message } from "./models/message";

/**
 * CustomStore implementation for chat messages
 * This service provides a DevExtreme-compatible data store for messages in a conversation
 */
@Injectable()
export class ChatMessageStore {
  private storeCache = new Map<string, CustomStore>();
  private currentUser: any;

  constructor(private chatConnection: ChatSignalRConnection) {}

  /**
   * Set the current user for the chat (used to determine message ownership)
   */
  public setCurrentUser(user: any): void {
    this.currentUser = user;
  }

  /**
   * Get a CustomStore instance for a specific conversation
   * This creates a DataStore that DevExtreme components can use
   */
  public getStore(
    conversationId: string,
    conversationType: ConversationType
  ): CustomStore {
    // Return cached store if it exists
    if (this.storeCache.has(conversationId)) {
      return this.storeCache.get(conversationId);
    }

    // Create a new store for this conversation
    const store = new CustomStore({
      key: "id",
      load: async (options) => {
        try {
          // Join the conversation channel on first load
          await this.chatConnection.joinConversation(conversationId);

          // Mark messages as read
          await this.chatConnection.markAsRead(conversationId);

          // Get messages from the connection
          const messagesObservable =
            this.chatConnection.getMessagesObservable(conversationId);
          const messages = await firstValueFrom(messagesObservable);

          // Convert messages to DxChat format
          return messages.map((message) =>
            ChatMessageAdapter.toDxChatMessage(message)
          );
        } catch (error) {
          console.error("Error loading messages:", error);
          throw error;
        }
      },
      insert: async (values) => {
        try {
          // Convert message from DxChat format
          const messageData = ChatMessageAdapter.fromDxChatMessage(values);

          let response: Message;

          // Send message based on conversation type
          if (conversationType === ConversationType.AiAssistant) {
            response = await this.chatConnection.sendMessageToAi(
              conversationId,
              messageData.text
            );
          } else {
            response = await this.chatConnection.sendMessage(
              conversationId,
              messageData.text
            );
          }

          // Return the newly created message in DxChat format
          return ChatMessageAdapter.toDxChatMessage(response);
        } catch (error) {
          console.error("Error inserting message:", error);
          throw error;
        }
      },
      // DevExtreme store requires these though we don't use them
      // since all updates come through SignalR
      update: async (key, values) => {
        console.warn("Update operation not supported for messages");
        return {};
      },
      remove: async (key) => {
        console.warn("Remove operation not supported for messages");
        return;
      },
    });

    // Cache the store
    this.storeCache.set(conversationId, store);
    return store;
  }

  /**
   * Request AI to regenerate a response
   */
  public async regenerateAiResponse(
    conversationId: string,
    messageId: string
  ): Promise<void> {
    try {
      await this.chatConnection.regenerateAiResponse(conversationId, messageId);
    } catch (error) {
      console.error("Error regenerating AI response:", error);
      throw error;
    }
  }

  /**
   * Create a DevExtreme DataSource for the specified conversation
   */
  public createDataSource(
    conversationId: string,
    conversationType: ConversationType
  ): any {
    const store = this.getStore(conversationId, conversationType);

    return {
      store: store,
      paginate: false,
      // DevExtreme will call this on push
      pushAggregationTimeout: 250,
      reshapeOnPush: true,
    };
  }
}
