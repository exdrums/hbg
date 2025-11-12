import { Component, Input, Output, EventEmitter } from '@angular/core';

export interface ChatMessage {
  id: number;
  timestamp: Date;
  author: { id: string; name: string };
  text: string;
}

@Component({
  selector: 'app-chat-assistant',
  templateUrl: './chat-assistant.component.html',
  styleUrls: ['./chat-assistant.component.scss']
})
export class ChatAssistantComponent {
  @Input() projectId: string | null = null;
  @Output() onMessageSent = new EventEmitter<string>();

  currentUser = {
    id: 'user-1',
    name: 'You'
  };

  aiUser = {
    id: 'ai-assistant',
    name: 'AI Assistant'
  };

  messages: ChatMessage[] = [
    {
      id: 1,
      timestamp: new Date(),
      author: this.aiUser,
      text: 'Hello! I can help you modify your jewelry design. Try saying things like "make it more modern" or "change the gemstone to ruby".'
    }
  ];

  onMessageEntered(e: any) {
    const messageText = e.message.text;

    // Add user message to chat
    this.messages = [...this.messages, {
      id: this.messages.length + 1,
      timestamp: new Date(),
      author: this.currentUser,
      text: messageText
    }];

    // Emit to parent for processing
    this.onMessageSent.emit(messageText);
  }

  addAiResponse(text: string) {
    this.messages = [...this.messages, {
      id: this.messages.length + 1,
      timestamp: new Date(),
      author: this.aiUser,
      text: text
    }];
  }
}
