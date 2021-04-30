using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DattingApplication.Extensions
{
    public static class ClaimPrincipleExtensions
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {
            var userName = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userName;

        }
    }
}
