using Microsoft.AspNetCore.Mvc;
using OidcServer.Models;

namespace OidcServer.Controllers
{
    public class AuthorizeController : Controller
    {
        public IActionResult Index(AuthenticationRequestModel model)
        {
            return View();
        }
    }
}
