/**
 * Chat view component for displaying and managing conversations
 * 
 * This component handles the actual messaging interface using DevExtreme's DxChat.
 * It manages message display, sending, editing, and real-time updates while
 * maintaining separation of concerns between UI and business logic.
 */

import { 
  Component, 
  Input, 
  Output, 
  EventEmitter, 
  OnInit, 
  OnDestroy, 
  ViewChild,
  ChangeDetectorRef 
} from '@angular/core';
import { DxChatComponent } from 'devextreme-angular/ui/chat';
import DataSource from 'devextreme/data/data_source';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { Conversation } from '../../model/conversation.model';
import { Message } from '../../model/message.model';
import { MessagesDataSourceService } from '../../data/messages.data-source';
import { AlertService } from '../../services/alert.service';
import { isAgentConversation, isSupportConversation } from '../../model/conversation-factory';
import { canDeleteMessage, canEditMessage } from '../../model/message-factory';
import { MessageType } from '../../model/message-type.enum';

/**
 * Configuration for the chat view
 */
export interface ChatViewConfig {
  /** Whether to show the header */
  showHeader: boolean;
  
  /** Whether to enable message editing */
  enableEditing: boolean;
  
  /** Whether to enable message deletion */
  enableDeletion: boolean;
  
  /** Whether to show typing indicators */
  showTypingIndicators: boolean;
  
  /** Whether to show read receipts */
  showReadReceipts: boolean;
  
  /** Custom header actions */
  headerActions?: Array<{
    text: string;
    icon?: string;
    onClick: () => void;
  }>;
  
  /** Message context menu items */
  contextMenuItems?: any[];
  
  /** Custom message template */
  messageTemplate?: string;
}

/**
 * User information for chat display
 */
export interface ChatUser {
  id: string;
  name: string;
  avatarUrl?: string;
}

@Component({
  selector: 'app-chat-view',
  standalone: false,
  templateUrl: './chat-view.component.html',
  styleUrls: ['./chat-view.component.scss']
})
export class ChatViewComponent implements OnInit, OnDestroy {
  @ViewChild('chat', { static: false }) chatComponent!: DxChatComponent;
  @ViewChild('contextMenu', { static: false }) contextMenu!: any;
  
  /** The conversation being displayed */
  @Input() conversation!: Conversation;
  
  /** Data source for messages */
  @Input() dataSource!: DataSource;
  
  /** Current user information */
  @Input() currentUser!: ChatUser;
  
  /** Map of user IDs to user information */
  @Input() userMap: Map<string, ChatUser> = new Map();
  
  /** Configuration for the chat view */
  @Input() config: ChatViewConfig = {
    showHeader: true,
    enableEditing: true,
    enableDeletion: true,
    showTypingIndicators: true,
    showReadReceipts: true
  };
  
  /** Height of the chat component */
  @Input() chatHeight = 'calc(100vh - 200px)';
  
  /** Event emitted when back button is clicked */
  @Output() backClicked = new EventEmitter<void>();
  
  /** Event emitted when settings button is clicked */
  @Output() settingsClicked = new EventEmitter<void>();
  
  /** Event emitted when a message is sent */
  @Output() messageSent = new EventEmitter<{ text: string; replyTo?: Message }>();
  
  /** Event emitted when a message is edited */
  @Output() messageEdited = new EventEmitter<{ messageId: string; newText: string }>();
  
  /** Event emitted when a message is deleted */
  @Output() messageDeleted = new EventEmitter<string>();
  
  private destroy$ = new Subject<void>();
  private typingSubject$ = new Subject<string>();
  
  /** Whether the conversation is read-only */
  isReadOnly = false;
  
  /** Read-only message to display */
  readOnlyMessage = '';
  
  /** Whether AI is currently thinking (for agent conversations) */
  isAIThinking = false;
  
  /** Users currently typing */
  typingUsers: string[] = [];
  
  /** Context menu items */
  contextMenuItems: any[] = [];
  
  /** Target element for context menu */
  contextMenuTarget: any = null;
  
  /** Selected message for context menu */
  selectedMessage: any = null;
  
  /** Whether to show image preview */
  showImagePreview = false;
  
  /** URL for image preview */
  previewImageUrl = '';

  constructor(
    private messagesService: MessagesDataSourceService,
    private alertService: AlertService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
    this.setupSubscriptions();
    this.checkReadOnlyStatus();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * Initialize component state
   */
  private initializeComponent(): void {
    // Setup typing indicator debouncing
    this.typingSubject$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(text => {
      if (text.trim()) {
        this.messagesService.onUserTyping(this.conversation.conversationId, text);
      }
    });

    // Setup context menu items
    this.updateContextMenuItems();
  }

  /**
   * Setup subscriptions for real-time updates
   */
  private setupSubscriptions(): void {
    // Subscribe to typing indicators
    if (this.config.showTypingIndicators) {
      this.messagesService.typing$.pipe(
        takeUntil(this.destroy$)
      ).subscribe(typingMap => {
        this.typingUsers = this.messagesService.getTypingUsers(this.conversation.conversationId)
          .filter(userId => userId !== this.currentUser.id);
        this.cdr.markForCheck();
      });
    }

    // Subscribe to AI thinking status for agent conversations
    if (isAgentConversation(this.conversation)) {
      // This would come from the agent connection service
      // For now, we'll simulate it
      this.dataSource.on('loadingChanged', (isLoading) => {
        this.isAIThinking = isLoading;
        this.cdr.markForCheck();
      });
    }
  }

  /**
   * Check if the conversation should be read-only
   */
  private checkReadOnlyStatus(): void {
    // Support conversations that are closed
    if (isSupportConversation(this.conversation) && !this.conversation.isActive) {
      this.isReadOnly = true;
      this.readOnlyMessage = 'This support ticket has been closed. You cannot send new messages.';
      return;
    }

    // Archived conversations
    if (!this.conversation.isActive) {
      this.isReadOnly = true;
      this.readOnlyMessage = 'This conversation has been archived.';
      return;
    }

    // User has left the conversation
    const hasLeft = !this.conversation.participantIds.includes(this.currentUser.id);
    if (hasLeft) {
      this.isReadOnly = true;
      this.readOnlyMessage = 'You have left this conversation.';
      return;
    }
  }

  /**
   * Update context menu items based on current state
   */
  private updateContextMenuItems(): void {
    this.contextMenuItems = [];

    if (this.config.enableEditing) {
      this.contextMenuItems.push({
        text: 'Edit',
        icon: 'edit',
        disabled: (item: any) => !this.canEdit(item)
      });
    }

    if (this.config.enableDeletion) {
      this.contextMenuItems.push({
        text: 'Delete',
        icon: 'trash',
        disabled: (item: any) => !this.canDelete(item)
      });
    }

    this.contextMenuItems.push(
      {
        text: 'Reply',
        icon: 'comment'
      },
      {
        text: 'Copy Text',
        icon: 'copy'
      }
    );

    // Add custom context menu items
    if (this.config.contextMenuItems) {
      this.contextMenuItems.push(...this.config.contextMenuItems);
    }
  }

  /**
   * Get conversation title for display
   */
  getConversationTitle(): string {
    if (this.conversation.type === 'Direct' && this.conversation.category === 'Contacts') {
      const otherUserId = this.conversation.participantIds.find(id => id !== this.currentUser.id);
      const otherUser = otherUserId ? this.userMap.get(otherUserId) : null;
      return otherUser?.name || 'Direct Message';
    }
    
    return this.conversation.title || 'Untitled Conversation';
  }

  /**
   * Get status text for the conversation
   */
  getStatusText(): string | null {
    if (isAgentConversation(this.conversation)) {
      return this.isAIThinking ? 'AI is thinking...' : 'AI Assistant';
    }
    
    if (isSupportConversation(this.conversation)) {
      const status = this.conversation.metadata?.support?.status || 'open';
      return status.charAt(0).toUpperCase() + status.slice(1);
    }
    
    return null;
  }

  /**
   * Get status CSS class
   */
  getStatusClass(): string {
    if (isAgentConversation(this.conversation)) {
      return this.isAIThinking ? 'status-thinking' : 'status-online';
    }
    
    if (isSupportConversation(this.conversation)) {
      const status = this.conversation.metadata?.support?.status || 'open';
      return `status-${status}`;
    }
    
    return '';
  }

  /**
   * Handle back button click
   */
  onBackClick(): void {
    this.backClicked.emit();
  }

  /**
   * Handle settings button click
   */
  onSettingsClick(): void {
    this.settingsClicked.emit();
  }

  /**
   * Handle message send event
   */
  onMessageSend(event: any): void {
    const text = event.message.text?.trim();
    if (!text) return;

    // Check for reply
    let replyTo: Message | undefined;
    if (this.selectedMessage && event.message.replyToMessageId) {
      replyTo = this.convertToMessage(this.selectedMessage);
    }

    this.messageSent.emit({ text, replyTo });
    
    // Clear typing indicator
    this.onTypingEnd();
  }

  /**
   * Handle typing start event
   */
  onTypingStart(): void {
    this.messagesService.startTyping(this.conversation.conversationId);
  }

  /**
   * Handle typing end event
   */
  onTypingEnd(): void {
    this.messagesService.stopTyping(this.conversation.conversationId);
  }

  /**
   * Handle message context menu
   */
  onMessageContextMenu(event: MouseEvent, message: any): void {
    event.preventDefault();
    
    this.selectedMessage = message;
    this.contextMenuTarget = event.target;
    
    // Update context menu items based on selected message
    this.updateContextMenuItems();
    
    // Show context menu
    if (this.contextMenu) {
      this.contextMenu.instance.option('position', {
        my: 'left top',
        at: 'left top',
        of: event,
        offset: { x: event.clientX, y: event.clientY }
      });
      this.contextMenu.instance.show();
    }
  }

  /**
   * Handle context menu item click
   */
  onContextMenuItemClick(event: any): void {
    const action = event.itemData.text;
    const message = this.selectedMessage;
    
    switch (action) {
      case 'Edit':
        this.editMessage(message);
        break;
        
      case 'Delete':
        this.deleteMessage(message);
        break;
        
      case 'Reply':
        this.replyToMessage(message);
        break;
        
      case 'Copy Text':
        this.copyMessageText(message);
        break;
    }
  }

  /**
   * Edit a message
   */
  private editMessage(message: any): void {
    // Implementation would show an edit dialog
    // For now, emit the event
    const messageId = message.customData?.id || message.id;
    this.messageEdited.emit({ messageId, newText: message.text });
  }

  /**
   * Delete a message
   */
  private deleteMessage(message: any): void {
    const messageId = message.customData?.id || message.id;
    this.messageDeleted.emit(messageId);
  }

  /**
   * Reply to a message
   */
  private replyToMessage(message: any): void {
    // Set the reply-to state in the chat component
    if (this.chatComponent) {
      // DevExtreme doesn't have built-in reply support
      // We'd need to implement this with custom UI
      this.alertService.showInfo('Reply feature coming soon!');
    }
  }

  /**
   * Copy message text to clipboard
   */
  private copyMessageText(message: any): void {
    navigator.clipboard.writeText(message.text).then(() => {
      this.alertService.showSuccess('Message copied to clipboard');
    }).catch(() => {
      this.alertService.showError('Failed to copy message');
    });
  }

  /**
   * Check if a message is from the current user
   */
  isOwnMessage(message: any): boolean {
    return message.author.id === this.currentUser.id;
  }

  /**
   * Check if user can edit a message
   */
  private canEdit(message: any): boolean {
    if (!this.config.enableEditing || this.isReadOnly) return false;
    
    const domainMessage = this.convertToMessage(message);
    return canEditMessage(domainMessage, this.currentUser.id);
  }

  /**
   * Check if user can delete a message
   */
  private canDelete(message: any): boolean {
    if (!this.config.enableDeletion || this.isReadOnly) return false;
    
    const domainMessage = this.convertToMessage(message);
    return canDeleteMessage(domainMessage, this.currentUser.id);
  }

  /**
   * Convert DevExtreme message to domain message
   */
  private convertToMessage(dxMessage: any): Message {
    return {
      messageId: dxMessage.customData?.id || dxMessage.id || '',
      conversationId: this.conversation.conversationId,
      senderUserId: dxMessage.author.id,
      content: dxMessage.text,
      type: dxMessage.customData?.type || MessageType.Text,
      sentAt: new Date(dxMessage.timestamp),
      editedAt: dxMessage.customData?.editedAt,
      isDeleted: dxMessage.customData?.isDeleted || false,
      readByUserIds: dxMessage.customData?.readByUserIds || [],
      replyToMessageId: dxMessage.customData?.replyToMessageId,
      metadata: dxMessage.customData?.metadata,
      isPending: dxMessage.customData?.isPending,
      error: dxMessage.customData?.error
    };
  }

  /**
   * Format message text with basic formatting
   */
  formatMessageText(text: string): string {
    // Basic formatting: URLs, mentions, etc.
    // This is a simplified version
    return text
      .replace(/\n/g, '<br>')
      .replace(/(https?:\/\/[^\s]+)/g, '<a href="$1" target="_blank">$1</a>')
      .replace(/@(\w+)/g, '<span class="mention">@$1</span>');
  }

  /**
   * Format file size for display
   */
  formatFileSize(bytes?: number): string {
    if (!bytes) return '';
    
    const units = ['B', 'KB', 'MB', 'GB'];
    let size = bytes;
    let unitIndex = 0;
    
    while (size >= 1024 && unitIndex < units.length - 1) {
      size /= 1024;
      unitIndex++;
    }
    
    return `${size.toFixed(1)} ${units[unitIndex]}`;
  }

  /**
   * Format message time
   */
  formatTime(timestamp: any): string {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', { 
      hour: 'numeric', 
      minute: '2-digit' 
    });
  }

  /**
   * Get typing indicator text
   */
  getTypingText(): string {
    if (this.typingUsers.length === 0) return '';
    
    const names = this.typingUsers.map(userId => {
      const user = this.userMap.get(userId);
      return user?.name || 'Someone';
    });
    
    if (names.length === 1) {
      return `${names[0]} is typing...`;
    } else if (names.length === 2) {
      return `${names[0]} and ${names[1]} are typing...`;
    } else {
      return `${names.length} people are typing...`;
    }
  }

  /**
   * Check if message is read by all participants
   */
  isMessageReadByAll(message: any): boolean {
    const readByCount = message.customData?.readByUserIds?.length || 0;
    const participantCount = this.conversation.participantIds.length;
    return readByCount >= participantCount;
  }

  /**
   * Get alert class based on type
   */
  getAlertClass(message: any): string {
    const alertType = message.customData?.metadata?.alert?.alertType || 'info';
    return `alert-${alertType}`;
  }

  /**
   * Get alert icon based on type
   */
  getAlertIcon(message: any): string {
    const alertType = message.customData?.metadata?.alert?.alertType || 'info';
    const iconMap: Record<string, string> = {
      'info': 'dx-icon-info',
      'success': 'dx-icon-check',
      'warning': 'dx-icon-warning',
      'error': 'dx-icon-close'
    };
    return iconMap[alertType] || 'dx-icon-info';
  }

  /**
   * Handle image click for preview
   */
  onImageClick(message: any): void {
    this.previewImageUrl = message.customData?.metadata?.file?.fileUrl || '';
    this.showImagePreview = true;
  }

  /**
   * Handle file download
   */
  onFileDownload(message: any): void {
    const fileUrl = message.customData?.metadata?.file?.fileUrl;
    if (fileUrl) {
      window.open(fileUrl, '_blank');
    }
  }
}