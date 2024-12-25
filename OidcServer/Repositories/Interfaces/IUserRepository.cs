using OidcServer.Models;

namespace OidcServer.Repositories.Interfaces
{
    public interface IUserRepository
    {
        User? FindByUsername(string username);
    }
}
