import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ChatDataSource } from '../../data/chat.data-source';
import { ConstructorHubService } from '../../services/constructor-hub.service';

@Component({
  selector: 'app-constructor-chat',
  templateUrl: './constructor-chat.component.html',
  styleUrls: ['./constructor-chat.component.scss'],
  providers: [ChatDataSource]
})
export class ConstructorChatComponent implements OnInit, OnDestroy {
  @Input() projectId: string | null = null;
  @Input() loading: boolean = false;

  chatDataSource: ChatDataSource;
  currentUser: any;

  private destroy$ = new Subject<void>();

  constructor(
    private hubService: ConstructorHubService,
    chatDataSource: ChatDataSource
  ) {
    this.chatDataSource = chatDataSource;
    this.currentUser = this.chatDataSource.getCurrentUser();
  }

  async ngOnInit() {
    // Ensure SignalR connection
    if (!this.hubService.isConnected) {
      try {
        await this.hubService.connect();
      } catch (error) {
        console.error('Failed to connect to chat hub:', error);
      }
    }

    // Set project ID if available
    if (this.projectId) {
      this.chatDataSource.setProjectId(this.projectId);
    }

    // Subscribe to chat responses
    this.hubService.onChatResponse$
      .pipe(takeUntil(this.destroy$))
      .subscribe(response => {
        if (response && response.message) {
          // The data source already handles adding the AI message
          console.log('Chat response received:', response);
        }
      });
  }

  onMessageEntered(e: any) {
    // The data source handles sending the message to SignalR
    console.log('Message entered:', e.message);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
