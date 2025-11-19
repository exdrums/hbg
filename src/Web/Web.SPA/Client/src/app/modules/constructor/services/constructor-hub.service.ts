import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { ConfigService } from '@app/core/services/config.service';
import { AuthService } from '@app/core/services/auth.service';
import { ImageUpdateEvent, ChatResponseEvent, ErrorEvent } from '../models';

@Injectable({
  providedIn: 'root'
})
export class ConstructorHubService {
  private hubConnection: HubConnection | null = null;
  private imageUpdateSubject = new BehaviorSubject<ImageUpdateEvent | null>(null);
  private chatResponseSubject = new BehaviorSubject<ChatResponseEvent | null>(null);
  private errorSubject = new BehaviorSubject<ErrorEvent | null>(null);
  private generationStartedSubject = new BehaviorSubject<boolean>(false);

  public onImageUpdate$: Observable<ImageUpdateEvent | null> = this.imageUpdateSubject.asObservable();
  public onChatResponse$: Observable<ChatResponseEvent | null> = this.chatResponseSubject.asObservable();
  public onError$: Observable<ErrorEvent | null> = this.errorSubject.asObservable();
  public onGenerationStarted$: Observable<boolean> = this.generationStartedSubject.asObservable();

  constructor(
    private config: ConfigService,
    private auth: AuthService
  ) {}

  async connect(): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      console.log('Constructor Hub already connected');
      return;
    }

    const token = this.auth.getToken();
    const hubUrl = `${this.config.hbgconstructor}/hubs/constructor`;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    // Register event handlers
    this.hubConnection.on('ReceiveImageUpdate', (data: ImageUpdateEvent) => {
      console.log('Received image update:', data);
      this.imageUpdateSubject.next(data);
      this.generationStartedSubject.next(false);
    });

    this.hubConnection.on('ReceiveChatResponse', (data: ChatResponseEvent) => {
      console.log('Received chat response:', data);
      this.chatResponseSubject.next(data);
    });

    this.hubConnection.on('ReceiveError', (data: ErrorEvent) => {
      console.error('Received error:', data);
      this.errorSubject.next(data);
      this.generationStartedSubject.next(false);
    });

    this.hubConnection.on('GenerationStarted', () => {
      console.log('Generation started');
      this.generationStartedSubject.next(true);
    });

    try {
      await this.hubConnection.start();
      console.log('Constructor Hub connected successfully');
    } catch (error) {
      console.error('Error connecting to Constructor Hub:', error);
      throw error;
    }
  }

  async sendChatMessage(projectId: string, message: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== HubConnectionState.Connected) {
      throw new Error('Hub connection is not established');
    }

    try {
      await this.hubConnection.invoke('SendChatMessage', projectId, message);
    } catch (error) {
      console.error('Error sending chat message:', error);
      throw error;
    }
  }

  async regenerateImage(configurationId: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== HubConnectionState.Connected) {
      throw new Error('Hub connection is not established');
    }

    try {
      this.generationStartedSubject.next(true);
      await this.hubConnection.invoke('RegenerateImage', configurationId);
    } catch (error) {
      console.error('Error regenerating image:', error);
      this.generationStartedSubject.next(false);
      throw error;
    }
  }

  async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        console.log('Constructor Hub disconnected');
      } catch (error) {
        console.error('Error disconnecting from Constructor Hub:', error);
      }
    }
  }

  get isConnected(): boolean {
    return this.hubConnection?.state === HubConnectionState.Connected;
  }
}
