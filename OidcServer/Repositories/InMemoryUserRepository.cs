using OidcServer.Models;
using OidcServer.Repositories.Interfaces;

namespace OidcServer.Repositories
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly User[] _users =
        [
            new User { UserName = "alice" },
            new User { UserName = "bob" },
            new User { UserName = "Huy" }
        ];
        public User? FindByUsername(string username)
        {
            return _users.FirstOrDefault(x => x.UserName.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
    }
}
