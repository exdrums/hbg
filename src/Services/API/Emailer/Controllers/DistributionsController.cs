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

namespace Emailer.Controllers;

[ApiController]
[Route("api/distributions")]
public class DistributionsController : Controller
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;
    private readonly SenderService service;

    public DistributionsController(AppDbContext appDbContext, IMapper mapper, SenderService service)
    {
        this.service = service;
        this.mapper = mapper;
        this.appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDx(DevExtremeLoadOptions loadOptions)
    {
        string userId = User.FindFirstValue("sub");

        var list = await appDbContext.GetDistribiutionList(userId);

        // AutoMapper
        var result = mapper.Map<List<DistributionDto>>(list);

        return Ok(DataSourceLoader.Load(result, loadOptions));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] DistributionDto dto) 
    {
        string userId = User.FindFirstValue("sub");

        if (dto.TemplateID < 1) throw new ArgumentNullException("Cannot create new Distribution. Provide TemplateID");
        if (dto.SenderID < 1) throw new ArgumentNullException("Cannot create new Distribution. Provide SenderID");

        // TODO: ValidateTemplateID and SenderID by UserID

        var dist = new Distribution()
        {
            TemplateID = dto.TemplateID,
            SenderID = dto.SenderID
        };

        mapper.Map(dto, dist);

        appDbContext.Distributions.Add(dist);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] DistributionDto dto)
    {
        string userId = User.FindFirstValue("sub");

        if (dto.TemplateID < 1) throw new ArgumentNullException("Cannot edit new Distribution. Provide TemplateID");
        if (dto.SenderID < 1) throw new ArgumentNullException("Cannot edit new Distribution. Provide SenderID");

        // TODO: ValidateTemplateID and SenderID by UserID

        Distribution? dist = null;
        try
        {
            dist = await appDbContext.GetDistribution(userId, dto.DistributionID);
        }
        catch(NotFoundException) { }

        if (dist is null) 
        {
            dist = new Distribution()
            {
                TemplateID = dto.TemplateID,
                SenderID = dto.SenderID
            };
            appDbContext.Distributions.Add(dist);
        }

        mapper.Map(dto, dist);

        // save changes
        await appDbContext.SaveChangesAsync();

        return Ok(dto);
    }

    [HttpDelete("{distributionId:int}")]
    public async Task<IActionResult> Delete(long distributionId)
    {
        string userId = User.FindFirstValue("sub");

        var distribution = await appDbContext.GetDistribution(userId, distributionId);

        appDbContext.Distributions.Remove(distribution);
        await appDbContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet("{distributionId:int}/start")]
    public async Task<IActionResult> StartDistribution(long distributionId)
    {
        string userId = User.FindFirstValue("sub");

        var dist = await appDbContext.GetDistribution(userId, distributionId, true);
        this.service.ProcessDistribution(dist);
        
        return Ok();
    }
}
