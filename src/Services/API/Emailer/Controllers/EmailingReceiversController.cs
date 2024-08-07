using System.Security.Claims;
using API.Emailer.Database;
using API.Emailer.Dtos;
using API.Emailer.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Utils;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Emailer.Controllers;

/// <summary>
/// Controller that used ti assign Receivers to the Distribution 
/// => also, manage their Email objects
/// </summary>
[ApiController]
[Route("api/distributions/{distributionId:int}/emailingreceivers")]
public class EmailingReceiversController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;
    public EmailingReceiversController(AppDbContext appDbContext, IMapper mapper)
    {
        this.mapper = mapper;
        this.appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmailingReceiversDx(DevExtremeLoadOptions loadOptions, long distributionId)
    {
        string userId = User.FindFirstValue("sub");

        var receivers = appDbContext.Receivers
            .Where(r => r.UserID == userId)
            .ProjectTo<EmailingReceiverDto>(mapper.ConfigurationProvider, new { distributionId = distributionId });

        var result = await DataSourceLoader.LoadAsync(receivers, loadOptions);

        return Ok(result);
    }

    /// <summary>
    /// Check existing Email object with this ids, add if necessary
    /// </summary>
    /// <param name="distributionId"></param>
    /// <param name="receiverId"></param>
    /// <returns></returns>
    [HttpPut("{receiverId:int}")]
    public async Task<IActionResult> PutEmailingReceiver(long distributionId, long receiverId, [FromBody] EmailingReceiverUpdateDto dto)
    {
        if (dto.Assigned)
            await AddEmail(distributionId, receiverId);
        else
            await RemoveEmail(distributionId, receiverId);
        return Ok();
    }

    private async Task AddEmail(long distributionId, long receiverId)
    {
        var exists = appDbContext.Emails.Any(r => r.DistributionID == distributionId && r.ReceiverID == receiverId);
        if (exists)
            return;

        var email = new Email()
        {
            DistributionID = distributionId,
            ReceiverID = receiverId,
            Status = EmailStatus.None
        };

        await appDbContext.Emails.AddAsync(email);
        await appDbContext.SaveChangesAsync();

        return;
    }

    private async Task RemoveEmail(long distributionId, long receiverId)
    {
        var email  = await appDbContext.Emails.FirstOrDefaultAsync(e => e.DistributionID == distributionId && e.ReceiverID == receiverId);
        if(email != null)
        {
            appDbContext.Emails.Remove(email);
            await appDbContext.SaveChangesAsync();
        }
    }
}
