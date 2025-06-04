import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, filter } from 'rxjs';
import notify from 'devextreme/ui/notify';
import { ChatWebSocketConnection } from '../connections/chat-ws.connection.service';

/**
 * Alert types supported by the system
 */
export enum AlertType {
  Info = 'info',
  Success = 'success',
  Warning = 'warning',
  Error = 'error',
  System = 'system',
  Permission = 'permission',
  Connection = 'connection',
  Maintenance = 'maintenance'
}

/**
 * Alert interface for type safety
 */
export interface Alert {
  id?: string;
  type: AlertType;
  content: string;
  title?: string;
  conversationId?: string;
  persistent?: boolean;
  autoHide?: boolean;
  timestamp?: Date;
  metadata?: any;
}

/**
 * Client-side alert management service
 * 
 * This service provides a comprehensive alert system that integrates with:
 * - DevExtreme notifications for UI display
 * - SignalR real-time alerts from the server
 * - Local client-side alerts and validations
 * 
 * Features:
 * - Different alert types with appropriate styling
 * - Persistent alerts that stay until dismissed
 * - Auto-hiding alerts with configurable duration
 * - Alert queuing and management
 * - Integration with chat functionality
 * - Connection status monitoring
 * 
 * The service bridges server-side alert events with client-side UI notifications
 * providing a seamless user experience for all types of system feedback.
 */
@Injectable()
export class AlertService {
  
  private _alerts$ = new BehaviorSubject<Alert[]>([]);
  private _currentAlerts: Alert[] = [];
  private _alertCounter = 0;

  // Default display durations for different alert types (in milliseconds)
  private readonly defaultDurations: Record<AlertType, number> = {
    [AlertType.Info]: 4000,
    [AlertType.Success]: 3000,
    [AlertType.Warning]: 6000,
    [AlertType.Error]: 8000,
    [AlertType.System]: 10000,
    [AlertType.Permission]: 6000,
    [AlertType.Connection]: 5000,
    [AlertType.Maintenance]: 15000
  };

  constructor(private chatConnection: ChatWebSocketConnection) {
    this.initializeSignalRAlerts();
  }

  /**
   * Observable stream of all active alerts
   */
  public get alerts$(): Observable<Alert[]> {
    return this._alerts$.asObservable();
  }

  /**
   * Get alerts of a specific type
   */
  public getAlertsByType(type: AlertType): Observable<Alert[]> {
    return this.alerts$.pipe(
      filter(alerts => alerts.some(alert => alert.type === type))
    );
  }

  /**
   * Get alerts for a specific conversation
   */
  public getConversationAlerts(conversationId: string): Observable<Alert[]> {
    return this.alerts$.pipe(
      filter(alerts => alerts.some(alert => alert.conversationId === conversationId))
    );
  }

  //#region Public Alert Methods

  /**
   * Shows an informational alert
   */
  public showInfo(message: string, title?: string, options?: Partial<Alert>): string {
    return this.showAlert({
      type: AlertType.Info,
      content: message,
      title,
      ...options
    });
  }

  /**
   * Shows a success alert
   */
  public showSuccess(message: string, title?: string, options?: Partial<Alert>): string {
    return this.showAlert({
      type: AlertType.Success,
      content: message,
      title,
      ...options
    });
  }

  /**
   * Shows a warning alert
   */
  public showWarning(message: string, title?: string, options?: Partial<Alert>): string {
    return this.showAlert({
      type: AlertType.Warning,
      content: message,
      title,
      ...options
    });
  }

  /**
   * Shows an error alert
   */
  public showError(message: string, title?: string, options?: Partial<Alert>): string {
    return this.showAlert({
      type: AlertType.Error,
      content: message,
      title,
      persistent: true, // Errors are persistent by default
      ...options
    });
  }

  /**
   * Shows a system alert
   */
  public showSystem(message: string, title?: string, options?: Partial<Alert>): string {
    return this.showAlert({
      type: AlertType.System,
      content: message,
      title: title || 'System Notice',
      persistent: true,
      ...options
    });
  }

  /**
   * Shows a permission denied alert
   */
  public showPermissionDenied(action: string, conversationId?: string): string {
    return this.showAlert({
      type: AlertType.Permission,
      content: `Permission denied: ${action}`,
      title: 'Access Denied',
      conversationId,
      persistent: true
    });
  }

  /**
   * Shows a connection status alert
   */
  public showConnectionAlert(isConnected: boolean, message?: string): string {
    const content = message || (isConnected ? 'Connected to chat service' : 'Connection lost');
    const type = isConnected ? AlertType.Success : AlertType.Warning;
    
    return this.showAlert({
      type,
      content,
      title: 'Connection Status',
      persistent: !isConnected,
      autoHide: isConnected
    });
  }

  /**
   * Shows a maintenance notification
   */
  public showMaintenance(message: string, startTime: Date, estimatedDuration: number): string {
    const durationText = estimatedDuration > 60 
      ? `${Math.round(estimatedDuration / 60)} hours` 
      : `${estimatedDuration} minutes`;
      
    return this.showAlert({
      type: AlertType.Maintenance,
      content: `${message} (Duration: ${durationText})`,
      title: 'Scheduled Maintenance',
      persistent: true,
      metadata: { startTime, estimatedDuration }
    });
  }

  //#endregion

  //#region Alert Management

  /**
   * Shows a generic alert with full customization
   */
  public showAlert(alert: Omit<Alert, 'id' | 'timestamp'>): string {
    const alertWithId: Alert = {
      ...alert,
      id: this.generateAlertId(),
      timestamp: new Date()
    };

    // Add to internal collection
    this._currentAlerts.push(alertWithId);
    this._alerts$.next([...this._currentAlerts]);

    // Show DevExtreme notification
    this.showDevExtremeNotification(alertWithId);

    // Auto-remove if not persistent
    if (!alert.persistent && !alert.autoHide === false) {
      this.scheduleAlertRemoval(alertWithId.id!, alert.type);
    }

    return alertWithId.id!;
  }

  /**
   * Dismisses a specific alert
   */
  public dismissAlert(alertId: string): void {
    const index = this._currentAlerts.findIndex(alert => alert.id === alertId);
    if (index !== -1) {
      this._currentAlerts.splice(index, 1);
      this._alerts$.next([...this._currentAlerts]);
    }
  }

  /**
   * Dismisses all alerts of a specific type
   */
  public dismissAlertsByType(type: AlertType): void {
    this._currentAlerts = this._currentAlerts.filter(alert => alert.type !== type);
    this._alerts$.next([...this._currentAlerts]);
  }

  /**
   * Dismisses all alerts for a specific conversation
   */
  public dismissConversationAlerts(conversationId: string): void {
    this._currentAlerts = this._currentAlerts.filter(alert => alert.conversationId !== conversationId);
    this._alerts$.next([...this._currentAlerts]);
  }

  /**
   * Dismisses all non-persistent alerts
   */
  public dismissTemporaryAlerts(): void {
    this._currentAlerts = this._currentAlerts.filter(alert => alert.persistent);
    this._alerts$.next([...this._currentAlerts]);
  }

  /**
   * Clears all alerts
   */
  public clearAllAlerts(): void {
    this._currentAlerts = [];
    this._alerts$.next([]);
  }

  //#endregion

  //#region SignalR Integration

  /**
   * Initialize SignalR alert handlers
   */
  private initializeSignalRAlerts(): void {
    // Subscribe to SignalR alerts
    this.chatConnection.alerts$.subscribe(alert => {
      if (alert) {
        this.handleSignalRAlert(alert);
      }
    });
  }

  /**
   * Handle incoming SignalR alerts
   */
  private handleSignalRAlert(signalRAlert: any): void {
    switch (signalRAlert.type) {
      case 'alert':
        this.handleGenericAlert(signalRAlert);
        break;
      case 'system':
        this.handleSystemAlert(signalRAlert);
        break;
      case 'permission':
        this.handlePermissionAlert(signalRAlert);
        break;
      case 'connection':
        this.handleConnectionAlert(signalRAlert);
        break;
      case 'maintenance':
        this.handleMaintenanceAlert(signalRAlert);
        break;
      default:
        console.warn('Unknown SignalR alert type:', signalRAlert.type);
    }
  }

  /**
   * Handle generic alerts from SignalR
   */
  private handleGenericAlert(alert: any): void {
    let alertType = AlertType.Info;
    let persistent = false;
    let autoHide = true;

    // Parse metadata if available
    if (alert.metadata) {
      try {
        const metadata = typeof alert.metadata === 'string' 
          ? JSON.parse(alert.metadata) 
          : alert.metadata;
        
        alertType = this.mapServerAlertType(metadata.alertType);
        persistent = metadata.isPersistent || false;
        autoHide = metadata.autoHide !== false;
      } catch (e) {
        console.warn('Failed to parse alert metadata:', e);
      }
    }

    this.showAlert({
      type: alertType,
      content: alert.content,
      conversationId: alert.conversationId,
      persistent,
      autoHide,
      metadata: alert.metadata
    });
  }

  /**
   * Handle system alerts from SignalR
   */
  private handleSystemAlert(alert: any): void {
    this.showSystem(alert.content, 'System Alert', {
      metadata: alert.metadata
    });
  }

  /**
   * Handle permission alerts from SignalR
   */
  private handlePermissionAlert(alert: any): void {
    this.showAlert({
      type: AlertType.Permission,
      content: alert.content,
      title: 'Permission Denied',
      conversationId: alert.conversationId,
      persistent: true
    });
  }

  /**
   * Handle connection alerts from SignalR
   */
  private handleConnectionAlert(alert: any): void {
    this.showConnectionAlert(alert.isConnected, alert.content);
  }

  /**
   * Handle maintenance alerts from SignalR
   */
  private handleMaintenanceAlert(alert: any): void {
    this.showMaintenance(
      alert.content, 
      alert.startTime, 
      alert.estimatedDuration
    );
  }

  //#endregion

  //#region Private Helper Methods

  /**
   * Generate a unique alert ID
   */
  private generateAlertId(): string {
    return `alert_${Date.now()}_${++this._alertCounter}`;
  }

  /**
   * Map server alert types to client alert types
   */
  private mapServerAlertType(serverType: string): AlertType {
    switch (serverType?.toLowerCase()) {
      case 'success': return AlertType.Success;
      case 'warning': return AlertType.Warning;
      case 'error': return AlertType.Error;
      case 'info':
      default: return AlertType.Info;
    }
  }

  /**
   * Show DevExtreme notification
   */
  private showDevExtremeNotification(alert: Alert): void {
    const notificationOptions: any = {
      message: alert.content,
      type: this.getDevExtremeNotificationType(alert.type),
      displayTime: alert.persistent ? 0 : this.defaultDurations[alert.type],
      closeOnClick: true
    };

    // Add title if provided
    if (alert.title) {
      notificationOptions.message = `<strong>${alert.title}</strong><br/>${alert.content}`;
    }

    // Show the notification
    notify(notificationOptions);
  }

  /**
   * Map alert types to DevExtreme notification types
   */
  private getDevExtremeNotificationType(alertType: AlertType): string {
    switch (alertType) {
      case AlertType.Success: return 'success';
      case AlertType.Warning: return 'warning';
      case AlertType.Error: return 'error';
      case AlertType.Permission: return 'error';
      case AlertType.System: return 'info';
      case AlertType.Connection: return 'warning';
      case AlertType.Maintenance: return 'warning';
      case AlertType.Info:
      default: return 'info';
    }
  }

  /**
   * Schedule automatic alert removal
   */
  private scheduleAlertRemoval(alertId: string, alertType: AlertType): void {
    const duration = this.defaultDurations[alertType];
    
    setTimeout(() => {
      this.dismissAlert(alertId);
    }, duration);
  }

  //#endregion
}