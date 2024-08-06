using System.Security.Claims;
using API.Emailer.Database;
using API.Emailer.Dtos;
using API.Emailer.Models;
using AutoMapper;
using Common.Exceptions;
using Common.Utils;
using DevExtreme.AspNet.Data;
using Emailer.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Emailer.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplatesController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;
    private readonly SenderService sender;

    public TemplatesController(AppDbContext appDbContext, IMapper mapper, SenderService sender)
    {
        this.sender = sender;
        this.mapper = mapper;
        this.appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDx(DevExtremeLoadOptions loadOptions)
    {
        string userId = User.FindFirstValue("sub");

        var templates = await appDbContext.GetTemplateList(userId);

        // AutoMapper
        var list = this.mapper.Map<List<TemplateListDto>>(templates);

        return Ok(DataSourceLoader.Load(list, loadOptions));
    }

    [HttpGet("{templateID:int}")]
    public async Task<IActionResult> GetWithContent(long templateID) 
    {   
        string userId = User.FindFirstValue("sub");

        var template = await appDbContext.GetTemplate(userId, templateID);

        var result = mapper.Map<TemplateDto>(template);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]TemplateListDto dto)
    {
        string userId = User.FindFirstValue("sub");

        var template = new Template();

        // Map dto to model
        mapper.Map(dto, template);

        template.TemplateID = 0;
        template.UserID = userId;
        template.Content ??= "";

        appDbContext.Templates.Add(template);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpPut("{templateID:int}")]
    public async Task<IActionResult> Put([FromBody]TemplateDto dto)
    {
        string userId = User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(dto.Name)) throw new ArgumentNullException("Cqannot put the Template. Name is required");

        Template? template = null;

        try 
        {
            template = await appDbContext.GetTemplate(userId, dto.TemplateID);
        }
        catch(NotFoundException) { }

        if(template is null) 
        {
            template = new Template();
        }

        // Map dto to model
        mapper.Map(dto, template);

        // save changes
        await appDbContext.SaveChangesAsync();
        // await sender.SendAsync("test");

        return Ok(dto);
    }
    
    [HttpDelete("{templateID:int}")]
    public async Task<IActionResult> Delete(long templateID)
    {
        string userId = User.FindFirstValue("sub");

        var template = await appDbContext.GetTemplate(userId, templateID);

        appDbContext.Templates.Remove(template);
        await appDbContext.SaveChangesAsync();
        
        return Ok();
    }
}
