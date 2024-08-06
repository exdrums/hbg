using API.Emailer.Dtos;
using API.Emailer.Models;
using AutoMapper;
using Emailer.Dtos;

namespace API.Emailer.Services;

public class AutoMapperProfile : Profile 
{
    public AutoMapperProfile()
    {
        CreateMap<Template, TemplateDto>()
            .ReverseMap()
            .ForMember(x => x.TemplateID, opts => opts.Ignore())
            .ForMember(x => x.UserID, opts => opts.Ignore())
            .ForAllMembers(opts => opts.AllowNull());

        CreateMap<Template, TemplateListDto>()
            .ReverseMap()
            .ForMember(x => x.TemplateID, opts => opts.Ignore())
            .ForMember(x => x.UserID, opts => opts.Ignore())

            .ForAllMembers(opts => opts.AllowNull());

        CreateMap<Distribution, DistributionDto>()
            .ReverseMap()
            .ForMember(x => x.DistributionID, opts => opts.Ignore())
            // .ForMember(x => x.UserID, opts => opts.Ignore())
            .ForAllMembers(opts => opts.AllowNull());

        CreateMap<Sender, SenderDto>()
            .ReverseMap()
            .ForMember(x => x.SenderID, opts => opts.Ignore())
            .ForMember(x => x.UserID, opts => opts.Ignore())
            .ForAllMembers(opts => opts.AllowNull());
    }
}
