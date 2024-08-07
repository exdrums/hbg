using System.Security.Claims;
using API.Emailer.Database;
using API.Emailer.Dtos;
using API.Emailer.Models;
using AutoMapper;
using Common.Exceptions;
using Common.Utils;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Emailer.Controllers;

[ApiController]
[Route("api/receivers")]
public class ReceiversController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;
    private readonly IOptionsSnapshot<EmailerAppSettings> emailerSettings;

    public ReceiversController(AppDbContext appDbContext, IMapper mapper, IOptionsSnapshot<EmailerAppSettings> emailerSettings)
    {
        this.emailerSettings = emailerSettings;
        this.mapper = mapper;
        this.appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDx(DevExtremeLoadOptions loadOptions)
    {
        string userId = User.FindFirstValue("sub");

        var list = await appDbContext.GetReceiversList(userId);

        // AutoMapper
        var result = mapper.Map<List<ReceiverDto>>(list);

        return Ok(DataSourceLoader.Load(result, loadOptions));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ReceiverDto dto) 
    {
        string userId = User.FindFirstValue("sub");

        var receiver = new Receiver()
        {
            UserID = userId
        };

        mapper.Map(dto, receiver);

        appDbContext.Receivers.Add(receiver);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] ReceiverDto dto)
    {
        string userId = User.FindFirstValue("sub");

        Receiver? receiver = null;
        try
        {
            receiver = await appDbContext.GetReceiver(userId, dto.ReceiverID);
        }
        catch(NotFoundException) { }

        if (receiver is null) 
        {
            receiver = new Receiver()
            {
                UserID = userId
            };
            appDbContext.Receivers.Add(receiver);
        }

        mapper.Map(dto, receiver);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpDelete("{receiverId:int}")]
    public async Task<IActionResult> Delete(long receiverId)
    {
        string userId = User.FindFirstValue("sub");

        var receiver = await appDbContext.GetReceiver(userId, receiverId);

        appDbContext.Receivers.Remove(receiver);
        await appDbContext.SaveChangesAsync();
        
        return Ok();
    }
}
