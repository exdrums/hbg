import DevExpress from "devextreme";
import { DeepPartial } from "devextreme/core";
import { dxTimeout } from "../utils/dx-utils";
import { WsConnection } from "../services/websocket/ws-connection";
import { CustomDataStore } from "./custom-data-store";

/**
 * Enhanced SignalR DataStore for real-time data synchronization
 * 
 * Features:
 * - Real-time push notifications through SignalR
 * - Support for different conversation types (Contacts, Support, Agent)
 * - Alert handling for non-persistent notifications
 * - Automatic cache management with real-time updates
 * - DevExtreme DataSource integration with server-side operations
 * 
 * This store bridges the gap between DevExtreme's data layer and
 * our SignalR-based real-time communication system.
 */
export class EnhancedSignalRDataStore<TGET, TKEY = number, TPOST = TGET> extends CustomDataStore<TGET, TKEY> {
    
    private _conversationType: string;
    private _alertHandlers: Map<string, (alert: any) => void> = new Map();
    private _isDestroyed: boolean = false;

    constructor(
        private connection: WsConnection,
        private keyProp: "conversationId" | "messageId" | "projectID" | "planID" | "articleID",
        private entity: "Conversation" | "Message" | "Project" | "Plan" | "Article",

    
        conversationType: string = "Contacts" // Contacts, Support, Agent
    ) {
        super({
            key: keyProp,
            loadMode: "raw",
            cacheRawData: true,
            load: (loadOptions: DevExpress.data.LoadOptions) => this.loadAction(loadOptions),
            insert: (value: TGET) => this.insertAction(value),
            update: (key, values) => this.updateAction(key, values),
            remove: (key) => this.removeAction(key),
            onPush: (changes) => this.handlePushChanges(changes)
        });

        this._conversationType = conversationType;
        void this.initializeHandlers();
    }

    /**
     * Gets the conversation type for this data store
     */
    public get conversationType(): string {
        return this._conversationType;
    }

    /**
     * Sets the conversation type and reinitializes handlers if needed
     */
    public set conversationType(value: string) {
        if (this._conversationType !== value) {
            this._conversationType = value;
            void this.initializeHandlers();
        }
    }

    /**
     * Registers an alert handler for specific alert types
     */
    public onAlert(alertType: string, handler: (alert: any) => void): void {
        this._alertHandlers.set(alertType, handler);
    }

    /**
     * Removes an alert handler
     */
    public offAlert(alertType: string): void {
        this._alertHandlers.delete(alertType);
    }

    /**
     * Destroys the data store and cleans up handlers
     */
    public destroy(): void {
        this._isDestroyed = true;
        this._alertHandlers.clear();
    }

    //#region Handler Registration

    /**
     * Initialize SignalR event handlers for this data store
     */
    private async initializeHandlers(): Promise<void> {
        if (this._isDestroyed) return;

        try {
            await this.connection.isConnectedPromise();
            
            // Register entity-specific handlers
            this.registerEntityHandlers();
            
            // Register alert handlers
            this.registerAlertHandlers();
            
            // Register DataSource-specific handlers for DevExtreme integration
            this.registerDataSourceHandlers();

            console.log(`Initialized handlers for ${this.entity} ${this._conversationType} data store`);
        } catch (error) {
            console.error(`Failed to initialize handlers for ${this.entity} data store:`, error);
        }
    }

    /**
     * Register handlers for entity-specific operations
     */
    private registerEntityHandlers(): void {
        // Standard CRUD handlers
        this.connection.addHandler({ 
            name: `loaded${this.entity}`, 
            handler: this.handleLoaded 
        });
        
        this.connection.addHandler({ 
            name: `added${this.entity}`, 
            handler: this.handleAdded 
        });
        
        this.connection.addHandler({ 
            name: `updated${this.entity}`, 
            handler: this.handleUpdated 
        });
        
        this.connection.addHandler({ 
            name: `removed${this.entity}`, 
            handler: this.handleRemoved 
        });

        // Conversation-specific handlers
        if (this.entity === "Conversation") {
            this.connection.addHandler({ 
                name: "ConversationCreated", 
                handler: this.handleConversationCreated 
            });
            
            this.connection.addHandler({ 
                name: "ConversationUpdated", 
                handler: this.handleConversationUpdated 
            });
            
            this.connection.addHandler({ 
                name: "ConversationArchived", 
                handler: this.handleConversationArchived 
            });
        }

        // Message-specific handlers
        if (this.entity === "Message") {
            this.connection.addHandler({ 
                name: "MessageReceived", 
                handler: this.handleMessageReceived 
            });
            
            this.connection.addHandler({ 
                name: "MessageEdited", 
                handler: this.handleMessageEdited 
            });
            
            this.connection.addHandler({ 
                name: "MessageDeleted", 
                handler: this.handleMessageDeleted 
            });
        }
    }

    /**
     * Register alert-specific handlers
     */
    private registerAlertHandlers(): void {
        this.connection.addHandler({ 
            name: "ReceiveAlert", 
            handler: this.handleAlert 
        });
        
        this.connection.addHandler({ 
            name: "ReceiveSystemAlert", 
            handler: this.handleSystemAlert 
        });
        
        this.connection.addHandler({ 
            name: "ReceivePermissionAlert", 
            handler: this.handlePermissionAlert 
        });
        
        this.connection.addHandler({ 
            name: "ConnectionStateChanged", 
            handler: this.handleConnectionStateChanged 
        });
    }

    /**
     * Register DevExtreme DataSource-specific handlers
     */
    private registerDataSourceHandlers(): void {
        this.connection.addHandler({ 
            name: "ReloadDataSource", 
            handler: this.handleReloadDataSource 
        });
        
        this.connection.addHandler({ 
            name: "PushDataSourceChanges", 
            handler: void this.handlePushDataSourceChanges 
        });
        
        // Conversation type-specific handlers
        this.connection.addHandler({ 
            name: "ConversationDataSourceInsert", 
            handler: void this.handleConversationDataSourceInsert 
        });
        
        this.connection.addHandler({ 
            name: "ConversationDataSourceUpdate", 
            handler: void this.handleConversationDataSourceUpdate 
        });
        
        this.connection.addHandler({ 
            name: "ConversationDataSourceRemove", 
            handler: void this.handleConversationDataSourceRemove 
        });
    }

    //#endregion

    //#region Entity Event Handlers

    private readonly handleLoaded = async (items: any[]): Promise<void> => {
        if (this._isDestroyed) return;
        
        await dxTimeout();
        console.log(`${this.entity} loaded:`, items.length, 'items');
        
        this.push(items.map(i => ({ 
            type: "insert", 
            data: i as DeepPartial<TGET>, 
            index: 0 
        })));
    };

    private readonly handleAdded = async (item: TGET): Promise<void> => {
        if (this._isDestroyed) return;
        
        await dxTimeout();
        console.log(`${this.entity} added:`, item);
        
        this.push([{ 
            type: "insert", 
            data: item as DeepPartial<TGET> 
        }]);
    };

    private readonly handleUpdated = async (item: TGET): Promise<void> => {
        if (this._isDestroyed) return;
        
        await dxTimeout();
        console.log(`${this.entity} updated:`, item);
        
        this.push([{ 
            type: "update", 
            data: item as DeepPartial<TGET>,
            key: (item as any)[this.keyProp]
        }]);
    };

    private readonly handleRemoved = async (key: TKEY): Promise<void> => {
        if (this._isDestroyed) return;
        
        await dxTimeout();
        console.log(`${this.entity} removed:`, key);
        
        this.push([{ 
            type: "remove", 
            key: key 
        }]);
    };

    //#endregion

    //#region Conversation-Specific Handlers

    private readonly handleConversationCreated = async (conversation: any): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Conversation") return;
        
        await dxTimeout();
        console.log('Conversation created:', conversation);
        
        this.push([{ 
            type: "insert", 
            data: conversation as DeepPartial<TGET> 
        }]);
    };

    private readonly handleConversationUpdated = async (conversation: any): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Conversation") return;
        
        await dxTimeout();
        console.log('Conversation updated:', conversation);
        
        this.push([{ 
            type: "update", 
            data: conversation as DeepPartial<TGET>,
            key: conversation.conversationId
        }]);
    };

    private readonly handleConversationArchived = async (conversationId: string): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Conversation") return;
        
        await dxTimeout();
        console.log('Conversation archived:', conversationId);
        
        this.push([{ 
            type: "remove", 
            key: conversationId as TKEY 
        }]);
    };

    //#endregion

    //#region Message-Specific Handlers

    private readonly handleMessageReceived = async (message: any): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Message") return;
        
        await dxTimeout();
        console.log('Message received:', message);
        
        this.push([{ 
            type: "insert", 
            data: message as DeepPartial<TGET> 
        }]);
    };

    private readonly handleMessageEdited = async (message: any): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Message") return;
        
        await dxTimeout();
        console.log('Message edited:', message);
        
        this.push([{ 
            type: "update", 
            data: message as DeepPartial<TGET>,
            key: message.messageId
        }]);
    };

    private readonly handleMessageDeleted = async (messageId: string): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Message") return;
        
        await dxTimeout();
        console.log('Message deleted:', messageId);
        
        // For deleted messages, we might want to update rather than remove
        // to show "Message deleted" placeholder
        this.push([{ 
            type: "update", 
            data: { 
                messageId, 
                content: "Message deleted", 
                isDeleted: true 
            } as unknown as DeepPartial<TGET>,
            key: messageId as TKEY
        }]);
    };

    //#endregion

    //#region Alert Handlers

    private readonly handleAlert = async (alert: any): Promise<void> => {
        if (this._isDestroyed) return;
        
        console.log('Alert received:', alert);
        
        // Extract alert type from metadata
        let alertType = 'info';
        if (alert.metadata) {
            try {
                const metadata = typeof alert.metadata === 'string' 
                    ? JSON.parse(alert.metadata) 
                    : alert.metadata;
                alertType = metadata.alertType || 'info';
            } catch (e) {
                console.warn('Failed to parse alert metadata:', e);
            }
        }
        
        // Call registered alert handler if exists
        const handler = this._alertHandlers.get(alertType);
        if (handler) {
            handler(alert);
        }
        
        // Call generic alert handler if exists
        const genericHandler = this._alertHandlers.get('*');
        if (genericHandler) {
            genericHandler(alert);
        }
    };

    private readonly handleSystemAlert = async (alert: any): Promise<void> => {
        if (this._isDestroyed) return;
        
        console.log('System alert received:', alert);
        
        const handler = this._alertHandlers.get('system');
        if (handler) {
            handler(alert);
        }
    };

    private readonly handlePermissionAlert = async (alert: any): Promise<void> => {
        if (this._isDestroyed) return;
        
        console.log('Permission alert received:', alert);
        
        const handler = this._alertHandlers.get('permission');
        if (handler) {
            handler(alert);
        }
    };

    private readonly handleConnectionStateChanged = async (isConnected: boolean, message?: string): Promise<void> => {
        if (this._isDestroyed) return;
        
        console.log('Connection state changed:', isConnected, message);
        
        const handler = this._alertHandlers.get('connection');
        if (handler) {
            handler({ isConnected, message });
        }
    };

    //#endregion

    //#region DataSource Handlers

    private readonly handleReloadDataSource = async (dataSourceName: string, parameters?: any): Promise<void> => {
        if (this._isDestroyed) return;
        
        console.log('Reload data source:', dataSourceName, parameters);
        
        // Reload this data source if it matches
        if (dataSourceName === this.entity || dataSourceName === this._conversationType) {
            this.clearRawDataCache();
            this.load();
        }
    };

    private readonly handlePushDataSourceChanges = async (dataSourceName: string, changes: any[]): Promise<void> => {
        if (this._isDestroyed) return;
        
        console.log('Push data source changes:', dataSourceName, changes);
        
        // Apply changes if they're for this data source
        if (dataSourceName === this.entity || dataSourceName === this._conversationType) {
            await dxTimeout();
            this.push(changes);
        }
    };

    private readonly handleConversationDataSourceInsert = async (conversation: any, conversationType: string): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Conversation") return;
        
        // Only handle if it's for our conversation type
        if (conversationType === this._conversationType) {
            await dxTimeout();
            this.push([{ 
                type: "insert", 
                data: conversation as DeepPartial<TGET> 
            }]);
        }
    };

    private readonly handleConversationDataSourceUpdate = async (conversation: any, conversationType: string): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Conversation") return;
        
        if (conversationType === this._conversationType) {
            await dxTimeout();
            this.push([{ 
                type: "update", 
                data: conversation as DeepPartial<TGET>,
                key: conversation.conversationId
            }]);
        }
    };

    private readonly handleConversationDataSourceRemove = async (conversationId: string, conversationType: string): Promise<void> => {
        if (this._isDestroyed || this.entity !== "Conversation") return;
        
        if (conversationType === this._conversationType) {
            await dxTimeout();
            this.push([{ 
                type: "remove", 
                key: conversationId as TKEY 
            }]);
        }
    };

    //#endregion

    //#region CRUD Actions

    /**
     * Load action with support for conversation type filtering
     */
    public async loadAction(loadOptions: DevExpress.data.LoadOptions): Promise<TGET[]> {
        await this.connection.isConnectedPromise();
        
        // Add conversation type parameter for Conversation entities
        const methodName = this.entity === "Conversation" 
            ? `load${this.entity}` 
            : `load${this.entity}`;
            
        const parameters = this.entity === "Conversation" 
            ? [loadOptions, this._conversationType]
            : [loadOptions, this.subjectId];
        
        const result = await this.connection.invoke(methodName, ...parameters);
        return result.data || result;
    }

    /**
     * Insert action for creating new entities
     */
    public async insertAction(value: TGET): Promise<TGET> {
        await this.connection.isConnectedPromise();
        
        const methodName = this.entity === "Conversation" 
            ? "CreateConversation"
            : `insert${this.entity}`;
            
        // For conversations, we need to handle the special creation method
        if (this.entity === "Conversation") {
            const conv = value as any;
            return await this.connection.invoke(methodName, 
                conv.title, 
                conv.participantIds || [], 
                this._conversationType
            );
        } else {
            return await this.connection.invoke(methodName, value, this.subjectId);
        }
    }

    /**
     * Update action for modifying existing entities
     */
    public async updateAction(key: any, values: any): Promise<void> {
        await this.connection.isConnectedPromise();
        
        const methodName = this.entity === "Conversation" 
            ? "UpdateConversation"
            : `update${this.entity}`;
            
        if (this.entity === "Conversation") {
            await this.connection.invoke(methodName, key, values);
        } else {
            await this.connection.invoke(methodName, key, values, this.subjectId);
        }
    }

    /**
     * Remove action for deleting entities
     */
    public async removeAction(key: any): Promise<void> {
        await this.connection.isConnectedPromise();
        
        const methodName = this.entity === "Conversation" 
            ? "ArchiveConversation"
            : `remove${this.entity}`;
            
        if (this.entity === "Conversation") {
            await this.connection.invoke(methodName, key);
        } else {
            await this.connection.invoke(methodName, key, this.subjectId);
        }
    }

    //#endregion

    //#region Helper Methods

    /**
     * Handle push changes with enhanced logging
     */
    private handlePushChanges(changes: any[]): void {
        console.log(`Push changes for ${this.entity} ${this._conversationType}:`, changes);
    }

    //#endregion
}