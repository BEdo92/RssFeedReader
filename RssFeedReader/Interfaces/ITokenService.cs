using RssFeedReader.Models;

namespace RssFeedReader.Interfaces;

public interface ITokenService
{
    string CreateToken(AppUser user);
}
