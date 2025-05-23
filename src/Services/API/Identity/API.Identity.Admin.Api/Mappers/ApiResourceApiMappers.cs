﻿using AutoMapper;

namespace API.Identity.Admin.Api.Mappers
{
    public static class ApiResourceApiMappers
    {
        static ApiResourceApiMappers()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<ApiResourceApiMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        public static T ToApiResourceApiModel<T>(this object source)
        {
            return Mapper.Map<T>(source);
        }
    }
}







