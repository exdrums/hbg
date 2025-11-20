import { Injectable } from "@angular/core";
import CustomStore from "devextreme/data/custom_store";
import DataSource from "devextreme/data/data_source";
import { ConstructorHubService } from "../services/constructor-hub.service";

export interface DxChatMessage {
    id: number;
    timestamp: Date;
    author: { id: string; name: string };
    text: string;
}

/**
 * Custom DataSource for chat messages that integrates with SignalR ConstructorHub
 */
@Injectable()
export class ChatDataSource extends DataSource {
    private messages: DxChatMessage[] = [];
    private projectId: string | null = null;
    private currentUser = { id: 'user-1', name: 'You' };
    private aiUser = { id: 'ai-assistant', name: 'AI Assistant' };

    constructor(private hubService: ConstructorHubService) {
        const customStore = new CustomStore({
            key: "id",
            load: () => {
                return Promise.resolve(this.messages);
            },
            insert: async (message: any) => {
                if (!this.projectId) {
                    throw new Error('Project ID not set');
                }

                // Create user message
                const userMessage: DxChatMessage = {
                    id: this.messages.length + 1,
                    timestamp: new Date(),
                    author: this.currentUser,
                    text: message.text
                };

                this.messages = [...this.messages, userMessage];

                // Send to SignalR hub
                try {
                    await this.hubService.sendChatMessage(this.projectId, message.text);
                } catch (error) {
                    console.error('Error sending chat message:', error);
                    throw error;
                }

                return userMessage;
            }
        });

        super({
            store: customStore,
            paginate: false
        });

        // Subscribe to chat responses from SignalR
        this.hubService.onChatResponse$.subscribe(response => {
            if (response && response.message) {
                this.addAiMessage(response.message);
            }
        });

        // Add welcome message
        this.addAiMessage('Hello! I can help you modify your jewelry design. Try saying things like "make it more modern" or "change the gemstone to ruby".');
    }

    setProjectId(projectId: string) {
        this.projectId = projectId;
        this.messages = [];
        this.addAiMessage('Hello! I can help you modify your jewelry design. Try saying things like "make it more modern" or "change the gemstone to ruby".');
        this.reload();
    }

    addAiMessage(text: string) {
        const aiMessage: DxChatMessage = {
            id: this.messages.length + 1,
            timestamp: new Date(),
            author: this.aiUser,
            text: text
        };

        this.messages = [...this.messages, aiMessage];
        this.reload();
    }

    getCurrentUser() {
        return this.currentUser;
    }
}
