using DattingApplication.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DattingApplication.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentPAge, 
                                                                           int itemsPerPage, 
                                                                           int totalItems, int totalPage)
        {
            var paginationnHeader = new PaginationHeader(currentPAge, itemsPerPage, totalItems, totalPage);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            response.Headers.Add("Pagination", JsonSerializer.Serialize(paginationnHeader));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
