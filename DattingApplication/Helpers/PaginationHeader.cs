using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DattingApplication.Helpers
{
    public class PaginationHeader
    {
        public PaginationHeader(int currentPage, int itemPerPage, int totalItem, int totalPages)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemPerPage;
            TotalItems = totalItem;
            TotalPages = totalPages;
        }

        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
