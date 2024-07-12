using API.Emailer.Dtos;
using API.Emailer.Models;
using AutoMapper;

namespace API.Emailer.Services;

public class AutoMapperProfile : Profile 
{
    public AutoMapperProfile()
    {
        CreateMap<Template, TemplateDto>()
            .ReverseMap()
            .ForMember(x => x.TemplateID, opts => opts.Ignore())
            .ForAllMembers(opts => opts.AllowNull());

        CreateMap<Template, TemplateListDto>()
            .ReverseMap()
            .ForMember(x => x.TemplateID, opts => opts.Ignore())
            .ForAllMembers(opts => opts.AllowNull());
    }
}
