import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { Subscription } from 'rxjs';
import { Conversation } from '../models/conversation';
import { Message } from '../models/message';
import { ChatService } from '../chat.service';
import { ConversationType } from '../models/conversation-type';

@Component({
  selector: 'hbg-conversations',
  templateUrl: './conversations.component.html',
  styleUrls: ['./conversations.component.scss']
})
export class ConversationsComponent implements OnInit, OnDestroy {
  @Input() selectedConversationId: string;
  @Output() conversationSelected = new EventEmitter<string>();
  @Output() newConversationCreated = new EventEmitter<Conversation>();
  
  public conversations: Conversation[] = [];
  public isLoading = true;
  public isConnected = false;
  public unreadMessageCounts = new Map<string, number>();
  public lastMessages = new Map<string, Message>();
  public searchText = '';
  public createDialogVisible = false;
  public createAiDialogVisible = false;
  public selectedParticipantIds: string[] = [];
  public availableUsers: any[] = []; // This would be populated from your user service
  
  private subscriptions: Subscription[] = [];

  constructor(private chatService: ChatService) {}

  ngOnInit(): void {
    // Subscribe to connection status
    this.subscriptions.push(
      this.chatService.isConnected().subscribe(connected => {
        this.isConnected = connected;
        if (connected) {
          this.loadConversations();
        }
      })
    );
    
    // Subscribe to conversations
    this.subscriptions.push(
      this.chatService.getConversations().subscribe(conversations => {
        this.conversations = this.sortConversations(conversations);
        this.isLoading = false;
        
        // If we have conversations but none selected, select the first one
        if (this.conversations.length > 0 && !this.selectedConversationId) {
          this.selectConversation(this.conversations[0].id);
        }
        
        // For each conversation, listen for unread messages
        this.conversations.forEach(conversation => {
          this.trackConversationUpdates(conversation.id);
        });
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  /**
   * Load all conversations the user has access to
   */
  private async loadConversations(): Promise<void> {
    this.isLoading = true;
    try {
      await this.chatService.loadAllConversations();
    } catch (error) {
      console.error('Error loading conversations:', error);
    }
  }

  /**
   * Track updates (new messages, unread counts) for a conversation
   */
  private trackConversationUpdates(conversationId: string): void {
    // Subscribe to messages for this conversation
    this.subscriptions.push(
      this.chatService.getMessages(conversationId).subscribe(messages => {
        // Update last message
        if (messages.length > 0) {
          this.lastMessages.set(conversationId, messages[messages.length - 1]);
        }
        
        // Calculate unread count (if this is not the selected conversation)
        if (conversationId !== this.selectedConversationId) {
          // In a real implementation, you'd track read status properly
          // This is simplified to show the concept
          const unreadCount = this.calcUnreadCount(messages, conversationId);
          this.unreadMessageCounts.set(conversationId, unreadCount);
        } else {
          // Currently selected conversation has no unread messages
          this.unreadMessageCounts.set(conversationId, 0);
        }
      })
    );
  }

  /**
   * Calculate the number of unread messages in a conversation
   */
  private calcUnreadCount(messages: Message[], conversationId: string): number {
    // This is a simplified implementation
    // A real implementation would track read receipts from the server
    const lastReadTime = localStorage.getItem(`last-read-${conversationId}`);
    if (!lastReadTime) return messages.length;
    
    const lastReadDate = new Date(lastReadTime);
    return messages.filter(m => new Date(m.timestamp) > lastReadDate).length;
  }

  /**
   * Sort conversations by last message time or unread status
   */
  private sortConversations(conversations: Conversation[]): Conversation[] {
    return [...conversations].sort((a, b) => {
      // First sort by unread messages
      const aUnread = this.unreadMessageCounts.get(a.id) || 0;
      const bUnread = this.unreadMessageCounts.get(b.id) || 0;
      if (aUnread > 0 && bUnread === 0) return -1;
      if (bUnread > 0 && aUnread === 0) return 1;
      
      // Then sort by last message time
      return new Date(b.lastMessageAt).getTime() - new Date(a.lastMessageAt).getTime();
    });
  }

  /**
   * Select a conversation
   */
  public selectConversation(conversationId: string): void {
    this.selectedConversationId = conversationId;
    this.conversationSelected.emit(conversationId);
    
    // Mark as read
    this.unreadMessageCounts.set(conversationId, 0);
    localStorage.setItem(`last-read-${conversationId}`, new Date().toISOString());
    this.chatService.markAsRead(conversationId);
  }

  /**
   * Filter conversations by search text
   */
  public get filteredConversations(): Conversation[] {
    if (!this.searchText) return this.conversations;
    
    return this.conversations.filter(c => 
      c.title.toLowerCase().includes(this.searchText.toLowerCase()) ||
      c.participants.some(p => p.name.toLowerCase().includes(this.searchText.toLowerCase()))
    );
  }

  /**
   * Get the formatted preview text for a conversation
   */
  public getPreviewText(conversationId: string): string {
    const lastMessage = this.lastMessages.get(conversationId);
    if (!lastMessage) return 'No messages yet';
    
    let preview = `${lastMessage.author.name}: ${lastMessage.text}`;
    if (preview.length > 50) {
      preview = preview.substring(0, 47) + '...';
    }
    return preview;
  }

  /**
   * Get the formatted time for a conversation
   */
  public getFormattedTime(conversationId: string): string {
    const conversation = this.conversations.find(c => c.id === conversationId);
    if (!conversation) return '';
    
    const lastMessage = this.lastMessages.get(conversationId);
    const time = lastMessage ? new Date(lastMessage.timestamp) : new Date(conversation.lastMessageAt);
    
    const now = new Date();
    const diffMs = now.getTime() - time.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));
    
    if (diffDays === 0) {
      // Today - show time
      return time.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } else if (diffDays === 1) {
      return 'Yesterday';
    } else if (diffDays < 7) {
      // Within a week - show day name
      return time.toLocaleDateString([], { weekday: 'short' });
    } else {
      // Older - show date
      return time.toLocaleDateString([], { month: 'short', day: 'numeric' });
    }
  }

  /**
   * Show dialog to create a new conversation
   */
  public showCreateDialog(): void {
    this.createDialogVisible = true;
    // In a real app, you would load available users here
    this.loadAvailableUsers();
  }

  /**
   * Show dialog to create a new AI conversation
   */
  public showCreateAiDialog(): void {
    this.createAiDialogVisible = true;
  }

  /**
   * Load available users (this would connect to your user service)
   */
  private loadAvailableUsers(): void {
    // Simplified implementation - in a real app, you would call your user service
    this.availableUsers = [
      { id: 'user1', name: 'John Doe' },
      { id: 'user2', name: 'Jane Smith' },
      { id: 'user3', name: 'Alex Johnson' }
    ];
  }

  /**
   * Create a new user-to-user conversation
   */
  public async createUserConversation(title: string): Promise<void> {
    this.createDialogVisible = false;
    
    if (!this.selectedParticipantIds.length) {
      console.warn('No participants selected');
      return;
    }
    
    try {
      const newConversation = await this.chatService.createConversation(
        this.selectedParticipantIds,
        title || 'New Conversation'
      );
      
      this.newConversationCreated.emit(newConversation);
      this.selectConversation(newConversation.id);
      
      // Reset form
      this.selectedParticipantIds = [];
    } catch (error) {
      console.error('Error creating conversation:', error);
    }
  }

  /**
   * Create a new AI assistant conversation
   */
  public async createAiConversation(title: string): Promise<void> {
    this.createAiDialogVisible = false;
    
    try {
      const newConversation = await this.chatService.createAiAssistantConversation(
        title || 'AI Assistant'
      );
      
      this.newConversationCreated.emit(newConversation);
      this.selectConversation(newConversation.id);
    } catch (error) {
      console.error('Error creating AI conversation:', error);
    }
  }

  /**
   * Get the conversation type label
   */
  public getConversationTypeLabel(type: ConversationType): string {
    return type === ConversationType.AiAssistant ? 'AI Assistant' : 'Chat';
  }

  /**
   * Cancel conversation creation
   */
  public cancelCreate(): void {
    this.createDialogVisible = false;
    this.createAiDialogVisible = false;
    this.selectedParticipantIds = [];
  }
}