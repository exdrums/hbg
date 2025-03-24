import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CustomStore } from 'devextreme-angular/common/data';
import { ChatSignalRConnection } from './chat-connection.service';
import { ConversationType } from './models/conversation-type';
import { Conversation } from './models/conversation';

/**
 * CustomStore implementation for chat conversations
 * This service provides a DevExtreme-compatible data store for conversations
 */
@Injectable()
export class ChatConversationStore {
  private store: CustomStore;

  constructor(private chatConnection: ChatSignalRConnection) {
    // Create the conversation store
    this.store = new CustomStore({
      key: 'id',
      load: async (options) => {
        try {
          // Load conversations from the server
          await this.chatConnection.isConnectedPromise();
          return await this.chatConnection.loadConversations();
        } catch (error) {
          console.error('Error loading conversations:', error);
          throw error;
        }
      },
      insert: async (values) => {
        try {
          // Create a new conversation based on type
          if (values.type === ConversationType.AiAssistant) {
            return await this.chatConnection.createAiAssistantConversation(values.title);
          } else {
            return await this.chatConnection.createConversation(values.participantIds, values.title);
          }
        } catch (error) {
          console.error('Error creating conversation:', error);
          throw error;
        }
      },
      // We don't support these operations directly - they would be handled through specific methods
      update: async (key, values) => {
        console.warn('Update operation not implemented for conversations');
        return {};
      },
      remove: async (key) => {
        console.warn('Remove operation not implemented for conversations');
        return;
      }
    });
  }

  /**
   * Get the CustomStore instance for conversations
   */
  public getStore(): CustomStore {
    return this.store;
  }

  /**
   * Create a conversation with the specified participants
   */
  public async createConversation(participantIds: string[], title?: string): Promise<Conversation> {
    return await this.chatConnection.createConversation(participantIds, title);
  }

  /**
   * Create an AI assistant conversation
   */
  public async createAiAssistantConversation(title: string = 'AI Assistant'): Promise<Conversation> {
    return await this.chatConnection.createAiAssistantConversation(title);
  }

  /**
   * Get all conversations
   */
  public async getConversations(): Promise<Conversation[]> {
    const conversationsObservable = this.chatConnection.conversations$;
    return await firstValueFrom(conversationsObservable);
  }

  /**
   * Create a DevExtreme DataSource for conversations
   */
  public createDataSource() {
    return {
      store: this.store,
      paginate: false,
      reshapeOnPush: true
    };
  }
}