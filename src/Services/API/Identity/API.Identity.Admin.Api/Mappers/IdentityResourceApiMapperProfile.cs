﻿using AutoMapper;
using API.Identity.Admin.Api.Dtos.IdentityResources;
using API.Identity.Admin.BusinessLogic.Dtos.Configuration;

namespace API.Identity.Admin.Api.Mappers
{
    public class IdentityResourceApiMapperProfile : Profile
    {
        public IdentityResourceApiMapperProfile()
        {
            // Identity Resources
            CreateMap<IdentityResourcesDto, IdentityResourcesApiDto>(MemberList.Destination)
                .ReverseMap();

            CreateMap<IdentityResourceDto, IdentityResourceApiDto>(MemberList.Destination)
                .ReverseMap();

            // Identity Resources Properties
            CreateMap<IdentityResourcePropertiesDto, IdentityResourcePropertyApiDto>(MemberList.Destination)
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IdentityResourcePropertyId))
                .ReverseMap();

            CreateMap<IdentityResourcePropertyDto, IdentityResourcePropertyApiDto>(MemberList.Destination);
            CreateMap<IdentityResourcePropertiesDto, IdentityResourcePropertiesApiDto>(MemberList.Destination);
        }
    }
}







