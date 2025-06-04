import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { DxTabPanelComponent } from 'devextreme-angular/ui/tab-panel';
import { Subject, takeUntil, Observable } from 'rxjs';
import { ChatWebSocketConnection } from '../../connections/chat-ws.connection.service';
import { AlertService } from '../../services/alert.service';
import { MessagesDataSourceService } from '../../data/messages.data-source';
import { Conversation, ConversationType, EnhancedConversationsDataSourceService } from '../../data/enhanced-conversations.data-source';
import { ConversationsDataSourceService } from '../../data/conversations.data-source';

/**
 * Interface for tab configuration
 */
interface ChatTab {
  id: ConversationType;
  text: string;
  icon: string;
  badge?: number;
  disabled?: boolean;
}

/**
 * Chat view modes
 */
enum ChatViewMode {
  ConversationList = 'list',
  Chat = 'chat'
}

/**
 * Main chat component providing tabbed interface for different conversation types
 * 
 * This component serves as the main entry point for the chat functionality and provides:
 * - Tabbed interface for Contacts, Support, and Agent conversations
 * - Conversation list view with real-time updates
 * - Chat interface using DevExtreme DxChat component
 * - Integration with SignalR for real-time messaging
 * - Alert system for user feedback and notifications
 * - Responsive design for different screen sizes
 * 
 * The component follows the MVVM pattern with reactive data binding
 * and comprehensive error handling throughout the chat workflow.
 */
@Component({
  selector: 'hbg-main-chat',
  standalone: false,
  templateUrl: './main-chat.component.html',
  styleUrls: ['./main-chat.component.scss']
})
export class MainChatComponent implements OnInit, OnDestroy {
  @ViewChild('tabPanel', { static: false }) tabPanel!: DxTabPanelComponent;

  private destroy$ = new Subject<void>();

  // Component state
  selectedTabIndex = 0;
  currentViewMode = ChatViewMode.ConversationList;
  selectedConversation: Conversation | null = null;
  ConversationType = ConversationType;
  
  // Dialog state
  showCreateDialog = false;
  createDialogType: ConversationType = ConversationType.Contacts;
  createConversationForm: any = {};
  availableUsers: any[] = [
    { id: 'user1', name: 'User One' },
    { id: 'user2', name: 'User Two' },
    { id: 'user3', name: 'User Three' }
  ]; // TODO: Load from user service

  // Current user (TODO: Get from auth service)
  currentUser = {
    id: 'current-user-id',
    name: 'Current User',
    avatarUrl: ''
  };

  // Tab configuration
  tabs: ChatTab[] = [
    { id: ConversationType.Contacts, text: 'Contacts', icon: 'user', badge: 0 },
    { id: ConversationType.Support, text: 'Support', icon: 'help', badge: 0 },
    { id: ConversationType.Agent, text: 'AI Agent', icon: 'favorites', badge: 0 }
  ];


  constructor(
    public conversationsService: EnhancedConversationsDataSourceService,
    private messagesService: MessagesDataSourceService,
    private alertService: AlertService,
    private chatConnection: ChatWebSocketConnection
  ) { 
    this.isConnected$ = this.chatConnection.isConnected$;
  }
  
  // Observables
  isConnected$: Observable<boolean>;

  ngOnInit(): void {
    this.initializeComponent();
    this.setupSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.conversationsService.destroy();
    this.messagesService.destroy();
  }

  //#region Component Initialization

  private initializeComponent(): void {
    // Set initial view mode
    this.currentViewMode = ChatViewMode.ConversationList;
    
    // Initialize create conversation form
    this.resetCreateConversationForm();
  }

  private setupSubscriptions(): void {
    // Update tab badges with conversation counts
    this.conversationsService.conversationCounts$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(counts => {
      this.tabs.forEach(tab => {
        tab.badge = counts[tab.id] || 0;
      });
    });

    // Handle selected conversation changes
    this.conversationsService.selectedConversation$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(conversation => {
      this.selectedConversation = conversation;
      if (conversation) {
        this.currentViewMode = ChatViewMode.Chat;
        this.messagesService.setCurrentConversation(conversation.conversationId);
      }
    });

    // Handle connection state changes
    this.isConnected$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(isConnected => {
      if (!isConnected) {
        this.alertService.showConnectionAlert(false, 'Connection to chat service lost');
      } else {
        this.alertService.showConnectionAlert(true, 'Connected to chat service');
      }
    });
  }

  //#endregion

  //#region Tab Management

  onTabSelectionChanged(event: any): void {
    const selectedTab = event.addedItems[0] as ChatTab;
    this.selectedTabIndex = this.tabs.findIndex(tab => tab.id === selectedTab.id);
    
    // Reset view mode when switching tabs
    this.currentViewMode = ChatViewMode.ConversationList;
    this.conversationsService.setSelectedConversation(null);
  }

  getCurrentConversationType(): ConversationType {
    return this.tabs[this.selectedTabIndex].id;
  }

  //#endregion

  //#region Conversation Management

  onConversationSelected(event: any): void {
    const conversation = event.data as Conversation;
    this.conversationsService.setSelectedConversation(conversation);
  }

  onRowPrepared(event: any): void {
    // Add custom styling for unread conversations
    if (event.data && event.data.unreadCount > 0) {
      event.rowElement.classList.add('unread-conversation');
    }
  }

  returnToConversationList(): void {
    this.currentViewMode = ChatViewMode.ConversationList;
    this.conversationsService.setSelectedConversation(null);
    this.messagesService.setCurrentConversation(null);
  }

  getConversationDisplayTitle(conversation: Conversation | null): string {
    if (!conversation) return '';
    
    if (conversation.type === 'Direct') {
      // For direct conversations, show other participant's name
      // TODO: Get actual user names from user service
      const otherParticipant = conversation.participantIds.find(id => id !== this.currentUser.id);
      return otherParticipant || 'Direct Message';
    }
    
    return conversation.title || 'Untitled Conversation';
  }

  //#endregion

  //#region Message Management

  getCurrentMessageDataSource(): any {
    const conversation = this.selectedConversation;
    if (!conversation) return null;
    
    return this.messagesService.getDataSource(conversation.conversationId);
  }

  onMessageSend(event: any): void {
    const conversation = this.selectedConversation;
    if (!conversation) return;

    const messageText = event.message.text;
    void this.messagesService.sendMessage(conversation.conversationId, messageText);
  }

  onTypingStart(): void {
    const conversation = this.selectedConversation;
    if (conversation) {
      void this.messagesService.startTyping(conversation.conversationId);
    }
  }

  onTypingEnd(): void {
    const conversation = this.selectedConversation;
    if (conversation) {
      void this.messagesService.stopTyping(conversation.conversationId);
    }
  }

  getTypingUsers(): string[] {
    const conversation = this.selectedConversation;
    if (!conversation) return [];
    
    return this.messagesService.getTypingUsers(conversation.conversationId)
      .filter(userId => userId !== this.currentUser.id);
  }

  getTypingIndicatorText(): string {
    const typingUsers = this.getTypingUsers();
    if (typingUsers.length === 0) return '';
    
    if (typingUsers.length === 1) {
      return `${typingUsers[0]} is typing...`;
    } else if (typingUsers.length === 2) {
      return `${typingUsers[0]} and ${typingUsers[1]} are typing...`;
    } else {
      return `${typingUsers.length} people are typing...`;
    }
  }

  isMessageRead(message: any): boolean {
    // TODO: Implement read receipt logic
    return true;
  }

  //#endregion

  //#region Conversation Creation

  showCreateConversationDialog(type: ConversationType): void {
    this.createDialogType = type;
    this.resetCreateConversationForm();
    this.showCreateDialog = true;
  }

  closeCreateDialog(): void {
    this.showCreateDialog = false;
    this.resetCreateConversationForm();
  }

  async createConversation(): Promise<void> {
    const form = this.createConversationForm;
    
    try {
      let conversation: Conversation | null = null;
      
      switch (this.createDialogType) {
        case ConversationType.Contacts:
          if (form.participants && form.participants.length === 1) {
            // Direct conversation
            conversation = await this.conversationsService.createDirectConversation(form.participants[0]);
          } else {
            // Group conversation
            conversation = await this.conversationsService.createConversation(
              form.title,
              form.participants.map(p => p.id) || [],
              ConversationType.Contacts
            );
          }
          break;
          
        case ConversationType.Support:
          conversation = await this.conversationsService.createSupportTicket(form.title);
          break;
          
        case ConversationType.Agent:
          conversation = await this.conversationsService.createAgentConversation(form.title);
          break;
      }
      
      if (conversation) {
        this.closeCreateDialog();
        this.conversationsService.setSelectedConversation(conversation);
      }
    } catch (error) {
      console.error('Failed to create conversation:', error);
    }
  }

  private resetCreateConversationForm(): void {
    this.createConversationForm = {
      title: '',
      participants: []
    };
  }

  getTitleLabel(): string {
    switch (this.createDialogType) {
      case ConversationType.Support: return 'Ticket Subject';
      case ConversationType.Agent: return 'Chat Topic';
      default: return 'Conversation Title';
    }
  }

  getTitlePlaceholder(): string {
    switch (this.createDialogType) {
      case ConversationType.Support: return 'Describe your issue...';
      case ConversationType.Agent: return 'What would you like to discuss?';
      default: return 'Enter conversation title...';
    }
  }

  //#endregion

  //#region Support-Specific Methods

  canCloseSupportTicket(): boolean {
    // TODO: Check if user has support agent permissions
    return this.selectedConversation?.isActive || false;
  }

  async closeSupportTicket(): Promise<void> {
    const conversation = this.selectedConversation;
    if (!conversation) return;
    
    try {
      await this.chatConnection.closeSupportTicket(conversation.conversationId, 'Resolved');
      this.alertService.showSuccess('Support ticket closed successfully', 'Ticket Closed');
    } catch (error) {
      console.error('Failed to close support ticket:', error);
      this.alertService.showError('Failed to close support ticket', 'Close Failed');
    }
  }

  //#endregion

  //#region Conversation Settings

  showConversationSettings(): void {
    // TODO: Implement conversation settings dialog
    this.alertService.showInfo('Conversation settings coming soon', 'Feature Not Available');
  }

  //#endregion
}