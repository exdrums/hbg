using Microsoft.AspNetCore.SignalR;

namespace Common.WebSocket;

public class UserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection) => connection!.User!.FindFirst("sub")!.Value;
}
