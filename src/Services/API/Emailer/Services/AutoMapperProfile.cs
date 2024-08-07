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

        CreateMap<Receiver, ReceiverDto>()
            .ReverseMap()
            .ForMember(x => x.ReceiverID, opts => opts.Ignore())
            .ForMember(x => x.UserID, opts => opts.Ignore())
            .ForAllMembers(opts => opts.AllowNull());

        long? distributionId = null;
        CreateProjection<Receiver, EmailingReceiverDto>()
            .ForMember(x => x.Assigned, opt => opt.MapFrom(src => src.Emails.Any(e => e.DistributionID == distributionId)));
    }
}
