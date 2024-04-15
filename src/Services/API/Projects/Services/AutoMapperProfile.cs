using AutoMapper;
using Projects.Dtos;
using Projects.Models;

namespace Projects.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Project, ProjectDto>()
                .ReverseMap()
                .ForMember(p => p.ProjectID, opts => opts.Ignore())
                .ForAllMembers(opts => opts.AllowNull());

            CreateMap<Plan, PlanDto>()
                .ReverseMap()
                .ForMember(p => p.PlanID, opts => opts.Ignore())
                .ForAllMembers(opts => opts.AllowNull());

            CreateMap<Article, ArticleDto>()
                .ReverseMap()
                .ForMember(p => p.ArticleID, opts => opts.Ignore())
                .ForAllMembers(opts => opts.AllowNull());
        }
    }
}