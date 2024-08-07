using System.Security.Claims;
using API.Emailer;
using API.Emailer.Database;
using API.Emailer.Dtos;
using API.Emailer.Models;
using AutoMapper;
using Common.Exceptions;
using Common.Utils;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Emailer.Controllers;

[ApiController]
[Route("api/senders")]
public class SendersController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;
    private readonly IOptionsSnapshot<EmailerAppSettings> emailerSettings;

    public SendersController(AppDbContext appDbContext, IMapper mapper, IOptionsSnapshot<EmailerAppSettings> emailerSettings)
    {
        this.emailerSettings = emailerSettings;
        this.mapper = mapper;
        this.appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDx(DevExtremeLoadOptions loadOptions)
    {
        string userId = User.FindFirstValue("sub");

        // temp seeding for default sender
        var settings = this.emailerSettings.Value;
        if(settings.EnableSeeding == true && !appDbContext.Senders.Any())
        {
             var defaultSender = new Sender()
            {
                UserID = userId,
                Name = "Default sender",
                Address = settings.DEFAULT_SENDER_ADDRESS,
                ServerAddress = settings.DEFAULT_SENDER_SERVER,
                Login = settings.DEFAULT_SENDER_ADDRESS,
                Passcode = settings.DEFAULT_SENDER_PASSCODE
            };
            appDbContext.Senders.Add(defaultSender);
            appDbContext.SaveChanges();
        }

        var list = await appDbContext.GetSendersList(userId);

        // AutoMapper
        var result = mapper.Map<List<SenderDto>>(list);

        return Ok(DataSourceLoader.Load(result, loadOptions));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] SenderDto dto) 
    {
        string userId = User.FindFirstValue("sub");

        var sender = new Sender()
        {
            UserID = userId
        };

        mapper.Map(dto, sender);

        appDbContext.Senders.Add(sender);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] SenderDto dto)
    {
        string userId = User.FindFirstValue("sub");


        Sender? sender = null;
        try
        {
            sender = await appDbContext.GetSender(userId, dto.SenderID);
        }
        catch(NotFoundException) { }

        if (sender is null) 
        {
            sender = new Sender()
            {
                UserID = userId
            };
            appDbContext.Senders.Add(sender);
        }

        mapper.Map(dto, sender);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpDelete("{senderId:int}")]
    public async Task<IActionResult> Delete(long senderId)
    {
        string userId = User.FindFirstValue("sub");

        var sender = await appDbContext.GetSender(userId, senderId);

        appDbContext.Senders.Remove(sender);
        await appDbContext.SaveChangesAsync();
        
        return Ok();
    }
}
