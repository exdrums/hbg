using API.Contacts.Dtos;
using API.Contacts.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Contacts.Services;

/// <summary>
/// Implementation of the real-time notification service using SignalR
/// </summary>
public class SignalRNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IConnectionManager _connectionManager;

    public SignalRNotificationService(
        IHubContext<ChatHub> hubContext,
        IConnectionManager connectionManager)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
    }

    public async Task NotifyMessageReceived(string conversationId, MessageDto message)
    {
        // Get all connection IDs for this conversation group
        var connections = _connectionManager.GetConnectionsForConversation(conversationId);
        
        if (connections.Any())
        {
            // Send the message to all clients connected to this conversation
            await _hubContext.Clients
                .Clients(connections)
                .SendAsync("ReceiveMessage", conversationId, message);
        }
    }

    public async Task NotifyUserStartedTyping(string conversationId, UserDto user)
    {
        var connections = _connectionManager.GetConnectionsForConversation(conversationId);
        
        if (connections.Any())
        {
            await _hubContext.Clients
                .Clients(connections)
                .SendAsync("UserStartedTyping", conversationId, user);
        }
    }

    public async Task NotifyUserStoppedTyping(string conversationId, UserDto user)
    {
        var connections = _connectionManager.GetConnectionsForConversation(conversationId);
        
        if (connections.Any())
        {
            await _hubContext.Clients
                .Clients(connections)
                .SendAsync("UserStoppedTyping", conversationId, user);
        }
    }

    public async Task NotifyAlertsChanged(string userId, IEnumerable<AlertDto> alerts)
    {
        var connections = _connectionManager.GetConnectionsForUser(userId);
        
        if (connections.Any())
        {
            await _hubContext.Clients
                .Clients(connections)
                .SendAsync("AlertsChanged", alerts);
        }
    }

    public async Task SubscribeToConversation(string connectionId, string conversationId)
    {
        _connectionManager.AddToConversation(connectionId, conversationId);
        await Task.CompletedTask;
    }

    public async Task UnsubscribeFromConversation(string connectionId, string conversationId)
    {
        _connectionManager.RemoveFromConversation(connectionId, conversationId);
        await Task.CompletedTask;
    }
}