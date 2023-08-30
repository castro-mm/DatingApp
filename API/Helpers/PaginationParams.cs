using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PaginationParams
    {
        public const int MaxPageSize = 50;

        public int PageNumber { get; set; } = 1;        

        public int _pageSize = 10;
        public int PageSize
        {
            get => this._pageSize;
            set => this._pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }      
    }
}