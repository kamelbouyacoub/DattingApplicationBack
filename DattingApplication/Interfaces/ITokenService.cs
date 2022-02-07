using DattingApplication.Entities;
using System.Threading.Tasks;

namespace DattingApplication.Interfaces
{
    public interface ITokenService
    {
        public Task<string> CreateToken(AppUser user);
    }
}
