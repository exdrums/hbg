import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ChatService } from '../chat.service';
import { Conversation } from '../models/conversation';

/**
 * Container component that combines the conversation list and chat components
 * This component can be used directly in your pages
 */
@Component({
  selector: 'hbg-chat-container',
  templateUrl: './chat-container.component.html',
  styleUrls: ['./chat-container.component.scss']
})
export class ChatContainerComponent implements OnInit, OnDestroy {
  // Configuration options
  @Input() fullscreen: boolean = true;
  @Input() initialConversationId: string;
  @Input() updateUrlParam: boolean = true;
  
  // Component state
  public selectedConversationId: string;
  public showMobileList: boolean = true;
  
  private subscriptions: Subscription[] = [];
  private routeParamName: string = 'conversationId';

  constructor(
    private chatService: ChatService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // If initial conversation ID is provided, use it
    if (this.initialConversationId) {
      this.selectedConversationId = this.initialConversationId;
      this.showMobileList = false;
    }
    
    // Otherwise, try to get it from route parameters
    if (this.updateUrlParam) {
      this.subscriptions.push(
        this.route.params.subscribe(params => {
          if (params[this.routeParamName]) {
            this.selectedConversationId = params[this.routeParamName];
            this.showMobileList = false;
          }
        })
      );
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  /**
   * Handle conversation selection from the list
   */
  public onConversationSelected(conversationId: string): void {
    this.selectedConversationId = conversationId;
    this.showMobileList = false;
    
    // Update URL if configured to do so
    if (this.updateUrlParam) {
      this.updateUrl(conversationId);
    }
  }

  /**
   * Handle new conversation creation
   */
  public onNewConversationCreated(conversation: Conversation): void {
    this.selectedConversationId = conversation.id;
    this.showMobileList = false;
    
    // Update URL if configured to do so
    if (this.updateUrlParam) {
      this.updateUrl(conversation.id);
    }
  }

  /**
   * Update URL with the selected conversation ID
   */
  private updateUrl(conversationId: string): void {
    // Use router to update the URL without a full navigation
    this.router.navigate(
      [], 
      {
        relativeTo: this.route,
        queryParams: { [this.routeParamName]: conversationId },
        queryParamsHandling: 'merge'
      }
    );
  }
  
  /**
   * Toggle mobile view between list and chat
   */
  public toggleMobileView(): void {
    this.showMobileList = !this.showMobileList;
  }
  
  /**
   * Navigate back to conversation list (mobile view)
   */
  public backToList(): void {
    this.showMobileList = true;
  }
}