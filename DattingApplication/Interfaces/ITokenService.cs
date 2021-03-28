using DattingApplication.Entities;
 

namespace DattingApplication.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(AppUser user);
    }
}
