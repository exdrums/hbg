using System.Security.Claims;
using API.Emailer.Database;
using API.Emailer.Models;
using AutoMapper;
using Common.Exceptions;
using Common.Utils;
using DevExtreme.AspNet.Data;
using Emailer.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Emailer.Controllers;

[ApiController]
[Route("api/senders")]
public class SendersController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public SendersController(AppDbContext appDbContext, IMapper mapper)
    {
        this.mapper = mapper;
        this.appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDx(DevExtremeLoadOptions loadOptions)
    {
        string userId = User.FindFirstValue("sub");

        var list = await appDbContext.GetDistribiutionList(userId);

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
