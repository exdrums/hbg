using Microsoft.AspNetCore.SignalR;

namespace API.Emailer.WebSocket;

public class EmailerHub : Hub<IEmailerHubClientActions>, IEmailerHubServerActions
{
    
    public override async Task OnConnectedAsync()
    {
        // add group with the userId
		await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

    public async Task TrackDistributions(long[] distIds)
    {
        foreach(var id in distIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());
        }
    }

    public async Task TrackDistribution(long distId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, distId.ToString());
    }
}


public interface IEmailerHubServerActions 
{
    /// <summary>
    /// Track Distributions for the Client
    /// Add the group for each Distribution id,
    /// so all Connection will be notified when the Distribution changed
    /// </summary>
    Task TrackDistributions(long[] distIds);
    Task TrackDistribution(long distId);
}

public interface IEmailerHubClientActions 
{
    /// <summary>
    /// Notify Client that distribution with this id was updated,
    /// to reload data on the client
    /// </summary>
    /// <param name="distrIds"></param>
    /// <returns></returns>
    Task DistributionUpdated(DistributionUpdatedHubDto updateDto);
}

public class DistributionUpdatedHubDto 
{
    public long DistributionId { get; set;}
    public int EmailsSent { get; set; }
    public int EmailsPending { get; set; }
    public int EmailsError{ get; set; }
}