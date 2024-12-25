using OidcServer.Models;
using OidcServer.Repositories.Interfaces;

namespace OidcServer.Repositories
{
    public class InMemoryCodeItemRepository : ICodeItemRepository
    {
        private readonly Dictionary<string, CodeItem> _items = new Dictionary<string, CodeItem>();
        public void Add(string code, CodeItem codeItem)
        {
            _items[code] = codeItem;
        }

        public CodeItem? FindByCode(string code)
        {
            _items.TryGetValue(code, out var codeItem);
            return codeItem;
        }

        public void Remove(string code)
        {
            _items.Remove(code);
        }
    }
}
