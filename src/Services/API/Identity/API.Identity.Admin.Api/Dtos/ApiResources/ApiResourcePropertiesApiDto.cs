﻿using System.Collections.Generic;

namespace API.Identity.Admin.Api.Dtos.ApiResources
{
    public class ApiResourcePropertiesApiDto
    {
        public ApiResourcePropertiesApiDto()
        {
            ApiResourceProperties = new List<ApiResourcePropertyApiDto>();
        }

        public List<ApiResourcePropertyApiDto> ApiResourceProperties { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }
    }
}







