using API.Emailer.Database;
using API.Emailer.Dtos;
using API.Emailer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Emailer.Controllers;

[ApiController]
[Route("[controller]")]
public class TemplatesController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public TemplatesController(AppDbContext appDbContext, IMapper mapper)
    {
        this.mapper = mapper;
        this.appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDx()
    {
        // TODOget UserID
        string userId = "";

        var templates = await appDbContext.GetTemplateListByUserId(userId);

        // AutoMapper
        var list = this.mapper.Map<List<TemplateListDto>>(templates);

        return Ok(list);
    }

    [HttpGet("{templateID:int}")]
    public async Task<IActionResult> GetWithContent(long templateID) 
    {   
        // TODOget UserID
        string userId = "";

        var template = await appDbContext.GetTemplateByUserId(userId);

        var result = mapper.Map<TemplateDto>(template);

        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody]TemplateListDto dto)
    {
        // TODOget UserID
        string userId = "";

        var template = await appDbContext.GetTemplateByUserId(userId);

        // Map dto to model
        mapper.Map(dto, template);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }
    
    [HttpDelete("{templateID:int}")]
    public async Task<IActionResult> Delete(long templateID)
    {
        // TODOget UserID
        string userId = "";

        var template = await appDbContext.GetTemplateByUserId(userId);

        appDbContext.Templates.Remove(template);
        await appDbContext.SaveChangesAsync();
        
        return Ok();
    }
}
