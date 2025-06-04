/**
 * Reusable conversation list component
 * 
 * This component displays a list of conversations using DevExtreme DataGrid
 * and can be configured for different conversation types (Contacts, Support, Agent).
 * Following the DRY principle, this component is reused across all tabs.
 */

import { Component, Input, Output, EventEmitter, OnInit, ViewChild } from '@angular/core';
import { DxDataGridComponent } from 'devextreme-angular/ui/data-grid';
import DataSource from 'devextreme/data/data_source';
import { AuthService } from '@app/core/services/auth.service';
import { ConversationType } from '../../model/conversation-type.enum';
import { Conversation } from '../../model/conversation.model';
import { getConversationDisplayTitle, isSupportConversation } from '../../model/conversation-factory';

/**
 * Configuration options for the conversation list
 */
export interface ConversationListConfig {
  /** Type of conversations to display */
  type: ConversationType;
  
  /** Whether to show the create button */
  showCreateButton: boolean;
  
  /** Custom create button text */
  createButtonText?: string;
  
  /** Custom columns configuration */
  customColumns?: any[];
  
  /** Whether to show unread indicators */
  showUnreadIndicator: boolean;
  
  /** Custom row CSS classes */
  rowCssClass?: (data: any) => string;
}

@Component({
  selector: 'app-conversation-list',
  standalone: false,
  templateUrl: './conversation-list.component.html',
  styleUrls: ['./conversation-list.component.scss']
})
export class ConversationListComponent implements OnInit {
  @ViewChild('dataGrid', { static: false }) dataGrid!: DxDataGridComponent;
  
  /** Data source for the grid */
  @Input() dataSource!: DataSource;
  
  /** Configuration for the list */
  @Input() config!: ConversationListConfig;
  
  /** Map of user IDs to display names */
  @Input() userNames: Map<string, string> = new Map();
  
  /** Current user ID for display logic */
  @Input() currentUserId!: string;
  
  /** Event emitted when a conversation is selected */
  @Output() conversationSelected = new EventEmitter<Conversation>();
  
  /** Event emitted when create button is clicked */
  @Output() createConversation = new EventEmitter<void>();
  
  /** Whether the list is empty */
  isEmpty = false;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // Subscribe to data source changes to update empty state
    this.dataSource.on('changed', () => {
      this.updateEmptyState();
    });
  }

  /**
   * Get the title for the list based on type
   */
  getTitle(): string {
    switch (this.config.type) {
      case ConversationType.Support:
        return 'Support Tickets';
      case ConversationType.Agent:
        return 'AI Conversations';
      default:
        return 'Conversations';
    }
  }

  /**
   * Get column configuration based on conversation type
   */
  getColumns(): any[] {
    const baseColumns = [
      {
        dataField: 'title',
        caption: this.config.type === ConversationType.Support ? 'Subject' : 'Conversation',
        cellTemplate: 'conversationTitleTemplate',
        sortOrder: undefined,
        sortIndex: undefined
      },
      {
        dataField: 'lastMessagePreview',
        caption: 'Last Message',
        cellTemplate: 'lastMessageTemplate',
        width: '40%'
      },
      {
        dataField: 'lastMessageAt',
        caption: 'Time',
        dataType: 'datetime',
        cellTemplate: 'timeTemplate',
        width: 100,
        sortOrder: 'desc',
        sortIndex: 0
      }
    ];

    // Add type-specific columns
    switch (this.config.type) {
      case ConversationType.Support:
        baseColumns.push(
          {
            dataField: 'metadata.support.status',
            caption: 'Status',
            cellTemplate: 'statusTemplate',
            width: "100",
            sortOrder: undefined,
            sortIndex: undefined
          },
          {
            dataField: 'metadata.support.priority',
            caption: 'Priority',
            cellTemplate: 'priorityTemplate',
            width: "80",
            sortOrder: undefined,
            sortIndex: undefined
          }
        );
        break;
        
      case ConversationType.Agent:
        baseColumns.push(
          {
            dataField: 'metadata.agent.agentType',
            caption: 'Agent',
            cellTemplate: 'agentTypeTemplate',
            width: "120",
            sortOrder: undefined,
            sortIndex: undefined
          },
          {
            dataField: 'metadata.agent.tokenUsage',
            caption: 'Usage',
            cellTemplate: 'tokenUsageTemplate',
            width: "100",
            sortOrder: undefined,
            sortIndex: undefined
          }
        );
        break;
    }

    // Apply custom columns if provided
    if (this.config.customColumns) {
      return this.config.customColumns;
    }

    return baseColumns;
  }

  /**
   * Handle row click event
   */
  onRowClick(event: any): void {
    if (event.data) {
      this.conversationSelected.emit(event.data);
    }
  }

  /**
   * Handle row prepared event for custom styling
   */
  onRowPrepared(event: any): void {
    if (!event.data) return;

    // Add unread class
    if (event.data.unreadCount > 0) {
      event.rowElement.classList.add('unread-conversation');
    }

    // Add custom CSS class if configured
    if (this.config.rowCssClass) {
      const cssClass = this.config.rowCssClass(event.data);
      if (cssClass) {
        event.rowElement.classList.add(cssClass);
      }
    }

    // Add type-specific classes
    if (isSupportConversation(event.data) && !event.data.isActive) {
      event.rowElement.classList.add('closed-conversation');
    }
  }

  /**
   * Handle content ready event
   */
  onContentReady(): void {
    this.updateEmptyState();
  }

  /**
   * Handle create button click
   */
  onCreateClick(): void {
    this.createConversation.emit();
  }

  /**
   * Update empty state based on data
   */
  private updateEmptyState(): void {
    const totalCount = this.dataGrid?.instance?.totalCount() || 0;
    this.isEmpty = totalCount === 0;
  }

  /**
   * Get display title for a conversation
   */
  getDisplayTitle(conversation: Conversation): string {
    return getConversationDisplayTitle(conversation, this.currentUserId, this.userNames);
  }

  /**
   * Get subtitle for a conversation
   */
  getSubtitle(conversation: Conversation): string | null {
    if (conversation.type === 'Group' && this.config.type === ConversationType.Contacts) {
      const count = conversation.participantIds?.length || 0;
      return `${count} participant${count !== 1 ? 's' : ''}`;
    }
    
    if (conversation.metadata?.support?.ticketNumber) {
      return conversation.metadata.support.ticketNumber;
    }
    
    return null;
  }

  /**
   * Get badge text for a conversation
   */
  getBadge(conversation: Conversation): string | null {
    if (conversation.metadata?.support?.assignedAgentId) {
      return 'Assigned';
    }
    
    if (conversation.metadata?.agent?.modelName) {
      return conversation.metadata.agent.modelName;
    }
    
    return null;
  }

  /**
   * Get badge CSS class
   */
  getBadgeClass(conversation: Conversation): string {
    if (conversation.metadata?.support) {
      return 'badge-support';
    }
    
    if (conversation.metadata?.agent) {
      return 'badge-agent';
    }
    
    return 'badge-default';
  }

  /**
   * Determine if conversation should show an icon
   */
  showIcon(conversation: Conversation): boolean {
    return conversation.type === 'Direct' || 
           conversation.type === 'Group' ||
           this.config.type === ConversationType.Agent;
  }

  /**
   * Get conversation icon class
   */
  getConversationIcon(conversation: Conversation): string {
    if (this.config.type === ConversationType.Agent) {
      return 'dx-icon-favorites';
    }
    
    if (conversation.type === 'Group') {
      return 'dx-icon-group';
    }
    
    return 'dx-icon-user';
  }

  /**
   * Check if a message preview is a system message
   */
  isSystemMessage(preview: string | undefined): boolean {
    if (!preview) return false;
    
    const systemPrefixes = [
      'User joined',
      'User left',
      'Title changed',
      'Conversation created',
      'Ticket closed',
      'Agent assigned'
    ];
    
    return systemPrefixes.some(prefix => preview.startsWith(prefix));
  }

  /**
   * Get empty message text based on type
   */
  getEmptyMessageText(): string {
    return this.config.type === ConversationType.Agent ? 
           'Start a conversation' : 
           'No messages yet';
  }

  /**
   * Get status text for support conversations
   */
  getStatusText(conversation: Conversation): string {
    return conversation.metadata?.support?.status || 'open';
  }

  /**
   * Get status CSS class
   */
  getStatusClass(conversation: Conversation): string {
    const status = conversation.metadata?.support?.status || 'open';
    return `status-${status}`;
  }

  /**
   * Get priority text for support conversations
   */
  getPriorityText(conversation: Conversation): string {
    return conversation.metadata?.support?.priority || 'medium';
  }

  /**
   * Get priority CSS class
   */
  getPriorityClass(conversation: Conversation): string {
    const priority = conversation.metadata?.support?.priority || 'medium';
    return `priority-${priority}`;
  }

  /**
   * Get agent type for AI conversations
   */
  getAgentType(conversation: Conversation): string {
    return conversation.metadata?.agent?.agentType || 'General';
  }

  /**
   * Get token usage information
   */
  getTokenUsage(conversation: Conversation): { percentage: number; text: string } | null {
    const usage = conversation.metadata?.agent?.tokenUsage;
    if (!usage) return null;
    
    const percentage = (usage.used / usage.limit) * 100;
    const remaining = usage.limit - usage.used;
    
    return {
      percentage,
      text: `${remaining} tokens`
    };
  }

  /**
   * Format time for display
   */
  formatTime(date: Date | string | undefined): string {
    if (!date) return '';
    
    const messageDate = new Date(date);
    const now = new Date();
    const diffInHours = (now.getTime() - messageDate.getTime()) / (1000 * 60 * 60);
    
    if (diffInHours < 24) {
      return messageDate.toLocaleTimeString('en-US', { 
        hour: 'numeric', 
        minute: '2-digit' 
      });
    }
    
    return '';
  }

  /**
   * Format date for display
   */
  formatDate(date: Date | string | undefined): string {
    if (!date) return '';
    
    const messageDate = new Date(date);
    const now = new Date();
    const diffInDays = Math.floor((now.getTime() - messageDate.getTime()) / (1000 * 60 * 60 * 24));
    
    if (diffInDays === 1) {
      return 'Yesterday';
    } else if (diffInDays < 7) {
      return messageDate.toLocaleDateString('en-US', { weekday: 'short' });
    } else {
      return messageDate.toLocaleDateString('en-US', { 
        month: 'short', 
        day: 'numeric' 
      });
    }
  }

  /**
   * Determine if date should be shown
   */
  shouldShowDate(date: Date | string | undefined): boolean {
    if (!date) return false;
    
    const messageDate = new Date(date);
    const now = new Date();
    const diffInHours = (now.getTime() - messageDate.getTime()) / (1000 * 60 * 60);
    
    return diffInHours >= 24;
  }

  /**
   * Get empty state icon
   */
  getEmptyStateIcon(): string {
    switch (this.config.type) {
      case ConversationType.Support:
        return 'dx-icon-help';
      case ConversationType.Agent:
        return 'dx-icon-favorites';
      default:
        return 'dx-icon-message';
    }
  }

  /**
   * Get empty state message
   */
  getEmptyStateMessage(): string {
    switch (this.config.type) {
      case ConversationType.Support:
        return 'No support tickets yet';
      case ConversationType.Agent:
        return 'No AI conversations yet';
      default:
        return 'No conversations yet';
    }
  }

  /**
   * Get empty state button text
   */
  getEmptyStateButtonText(): string {
    switch (this.config.type) {
      case ConversationType.Support:
        return 'Create Support Ticket';
      case ConversationType.Agent:
        return 'Start AI Chat';
      default:
        return 'Start New Conversation';
    }
  }
}