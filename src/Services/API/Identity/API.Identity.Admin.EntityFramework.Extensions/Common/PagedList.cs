﻿using System.Collections.Generic;

namespace API.Identity.Admin.EntityFramework.Extensions.Common
{
    public class PagedList<T> where T : class 
    {
        public PagedList()
        {
            Data = new List<T>();
        }

        public List<T> Data { get; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }
    }
}
