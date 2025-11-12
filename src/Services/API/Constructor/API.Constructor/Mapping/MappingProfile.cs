using AutoMapper;
using API.Constructor.Models.Entities;
using API.Constructor.Models.DTOs;
using System.Text.Json;

namespace API.Constructor.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ConstructorProject mappings
            CreateMap<ConstructorProject, ProjectDto>();
            CreateMap<CreateProjectDto, ConstructorProject>();
            CreateMap<UpdateProjectDto, ConstructorProject>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ProjectConfiguration mappings
            CreateMap<ProjectConfiguration, ConfigurationDto>()
                .ForMember(dest => dest.FormData,
                    opt => opt.MapFrom(src => JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(src.FormDataJson)));

            CreateMap<CreateConfigurationDto, ProjectConfiguration>()
                .ForMember(dest => dest.FormDataJson,
                    opt => opt.MapFrom(src => JsonSerializer.Serialize(src.FormData)));

            CreateMap<UpdateConfigurationDto, ProjectConfiguration>()
                .ForMember(dest => dest.FormDataJson,
                    opt => opt.MapFrom(src => JsonSerializer.Serialize(src.FormData)))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // GeneratedImage mappings
            CreateMap<GeneratedImage, ImageDto>();

            // ChatInteraction mappings
            CreateMap<ChatInteraction, ChatMessageDto>();
        }
    }
}
