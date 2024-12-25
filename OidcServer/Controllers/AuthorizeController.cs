using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using OidcServer.Helpers;
using OidcServer.Models;
using OidcServer.Repositories.Interfaces;
using System.Security.Claims;

namespace OidcServer.Controllers
{
    public class AuthorizeController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ICodeItemRepository _codeItemRepository;

        public AuthorizeController(IUserRepository userRepository, ICodeItemRepository codeItemRepository)
        {
            _userRepository = userRepository;
            _codeItemRepository = codeItemRepository;
        }

        public IActionResult Index(AuthenticationRequestModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult Authorize(AuthenticationRequestModel model, string user, string[] scope)
        {
            if (_userRepository.FindByUsername(user) == null)
            {
                return View("UserNotFound");
            }
            string code = GeneratedCode();
            _codeItemRepository.Add(code, new CodeItem
            {
                UserName = user,
                Scopes = scope,
                AuthenticationRequestModel = model
            });
            var modelCodeFlow = new CodeFlowResponseViewModel()
            {
                Code = code,
                RedirectUri = model.RedirectUri,
                State = model.State
            };

            return View("SubmitForm", modelCodeFlow);
        }

        [Route("token")]
        [HttpPost]
        public IActionResult ReturnTokens(string grant_type, string code, string redirect_uri)
        {
            if (grant_type != "authorization_code")
            {
                return BadRequest();
            }

            var codeItem = _codeItemRepository.FindByCode(code);
            if (codeItem == null)
            {
                return BadRequest();
            }

            if (codeItem.AuthenticationRequestModel.RedirectUri != redirect_uri)
            {
                return BadRequest();
            }

            _codeItemRepository.Remove(code);

            var jwk = JwkLoader.LoadFromDefault();

            var model = new AuthenticationResponseModel()
            {
                AccessToken = GeneratedAccessToken(codeItem.UserName, string.Join(",", codeItem.Scopes), codeItem.AuthenticationRequestModel.ClientId, codeItem.AuthenticationRequestModel.Nonce, jwk),
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshToken = GeneratedRefreshToken(),
                IdToken = GeneratedIdToken(codeItem.UserName, string.Join(",", codeItem.Scopes), codeItem.AuthenticationRequestModel.ClientId, codeItem.AuthenticationRequestModel.Nonce, jwk),
            };

            return Json(model);
        }

        private string GeneratedIdToken(string userId, string scope, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // https://openid.net/specs/openid-connect-core-1_0.html#IDToken
            // we can return some claims defined here: https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId)
            };

            var idToken = JwtGenerator.GenerateJWTToken(
                20 * 60,
                "https://localhost:5001/",
                audience,
                nonce,
                claims,
                jsonWebKey
                );


            return idToken;
        }

        private string GeneratedAccessToken(string userId, string scope, string audience, string nonce, JsonWebKey jsonWebKey)
        {
            // access_token can be the same as id_token, but here we might have different values for expirySeconds so we use 2 different functions

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new("scope", scope) // Jeg vet ikke hvorfor JwtRegisteredClaimNames inneholder ikke "scope"??? Det har kun OIDC ting?  https://datatracker.ietf.org/doc/html/rfc8693#name-scope-scopes-claim
            };
            var idToken = JwtGenerator.GenerateJWTToken(
                20 * 60,
                "https://localhost:5001/",
                audience,
                nonce,
                claims,
                jsonWebKey
                );

            return idToken;
        }

        private static string GeneratedRefreshToken()
        {
            return GeneratedCode();
        }

        private static string GeneratedCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 32)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
