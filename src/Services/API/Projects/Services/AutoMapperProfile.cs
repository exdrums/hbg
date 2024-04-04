using AutoMapper;
using Projects.Dtos;

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
        }
    }
}