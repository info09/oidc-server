using OidcServer.Models;

namespace OidcServer.Repositories.Interfaces
{
    public interface ICodeItemRepository
    {
        CodeItem? FindByCode(string code);
        void Add(string code, CodeItem codeItem);
        void Remove(string code);
    }
}
