import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { DxChatComponent } from 'devextreme-angular';
import { Subscription, filter, tap } from 'rxjs';
import DataSource from 'devextreme/data/data_source';
import { Conversation } from '../models/conversation';
import { User } from '../models/user';
import { Alert } from '../models/alert';
import { ChatService } from '../chat.service';
import { ConversationType } from '../models/conversation-type';

@Component({
  selector: 'hbg-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class HbgChatComponent implements OnInit, OnDestroy {
  @ViewChild(DxChatComponent, { static: false }) dxChatComponent: DxChatComponent;
  
  // Inputs for component configuration
  @Input() conversationId: string;
  @Input() isDialog: boolean = false;
  @Input() showToolbar: boolean = true;
  
  // Output events
  @Output() closeChat = new EventEmitter<void>();
  
  // Component state
  public isConnected: boolean = false;
  public isLoading: boolean = true;
  public conversation: Conversation;
  public currentUser: User;
  public typingUsers: User[] = [];
  public alerts: Alert[] = [];
  public messageDataSource: DataSource;
  public shouldDisplayTypingIndicator: boolean = false;
  public typingTimeout: any;
  
  // Private subscriptions for cleanup
  private subscriptions: Subscription[] = [];

  constructor(private chatService: ChatService) {}

  ngOnInit(): void {
    // Get current user
    this.currentUser = this.chatService.getCurrentUser();
    
    // Subscribe to connection status
    const connectionSub = this.chatService.isConnected()
      .pipe(
        tap(connected => {
          this.isConnected = connected;
          if (connected && this.conversationId) {
            this.loadConversation();
          }
        })
      )
      .subscribe();
    this.subscriptions.push(connectionSub);
    
    // Subscribe to alerts
    const alertsSub = this.chatService.getAlerts()
      .subscribe(alerts => {
        this.alerts = alerts;
      });
    this.subscriptions.push(alertsSub);
    
    // If we already have a conversation ID, load it
    if (this.conversationId) {
      this.loadConversation();
    }
  }

  ngOnDestroy(): void {
    // Clear all subscriptions
    this.subscriptions.forEach(s => s.unsubscribe());
    
    // Clear typing timeout
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
  }

  /**
   * Load the conversation details and setup chat
   */
  private async loadConversation(): Promise<void> {
    try {
      this.isLoading = true;
      
      // Get conversation details
      this.conversation = await this.chatService.getConversation(this.conversationId);
      
      if (!this.conversation) {
        console.error(`Conversation with ID ${this.conversationId} not found`);
        return;
      }
      
      // Create message data source
      this.messageDataSource = this.chatService.createMessageDataSource(
        this.conversationId,
        this.conversation.type
      );
      
      // Subscribe to typing users
      const typingSub = this.chatService.getTypingUsers(this.conversationId)
        .subscribe(users => {
          this.typingUsers = users.filter(u => u.id !== this.currentUser.id);
          this.shouldDisplayTypingIndicator = this.typingUsers.length > 0;
        });
      this.subscriptions.push(typingSub);
      
      // Mark messages as read
      await this.chatService.markAsRead(this.conversationId);
      
      this.isLoading = false;
    } catch (error) {
      console.error('Error loading conversation:', error);
      this.isLoading = false;
    }
  }

  /**
   * Handle message entered event from DevExtreme Chat
   */
  public onMessageEntered(e: any): void {
    // Clear typing indicator
    this.onTypingEnd();
    
    // The DxChat component will handle sending the message through the DataSource
  }

  /**
   * Handle typing started event from DevExtreme Chat
   */
  public onTypingStart(e: any): void {
    // Clear any existing timeout
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
    }
    
    // Notify server that user started typing
    this.chatService.notifyTypingStarted(this.conversationId);
    
    // Set a timeout to clear typing indicator if user stops typing
    this.typingTimeout = setTimeout(() => this.onTypingEnd(), 5000);
  }

  /**
   * Handle typing ended event
   */
  public onTypingEnd(): void {
    // Clear timeout
    if (this.typingTimeout) {
      clearTimeout(this.typingTimeout);
      this.typingTimeout = null;
    }
    
    // Notify server that user stopped typing
    this.chatService.notifyTypingStopped(this.conversationId);
  }

  /**
   * Regenerate an AI response
   */
  public async regenerateAiResponse(messageId: string): Promise<void> {
    if (this.conversation.type !== ConversationType.AiAssistant) {
      return;
    }
    
    try {
      await this.chatService.regenerateAiResponse(this.conversationId, messageId);
    } catch (error) {
      console.error('Error regenerating AI response:', error);
    }
  }

  /**
   * Close the chat (for dialog mode)
   */
  public onCloseChat(): void {
    this.closeChat.emit();
  }

  /**
   * Create a new conversation
   */
  public async createNewConversation(isAi: boolean = false): Promise<void> {
    try {
      let newConversation: Conversation;
      
      if (isAi) {
        newConversation = await this.chatService.createAiAssistantConversation();
      } else {
        // In a real app, you would have a user selection UI
        const participantIds = ['some-user-id']; 
        newConversation = await this.chatService.createConversation(participantIds);
      }
      
      // Navigate to the new conversation
      this.conversationId = newConversation.id;
      await this.loadConversation();
    } catch (error) {
      console.error('Error creating conversation:', error);
    }
  }
}