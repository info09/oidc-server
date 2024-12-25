namespace OidcServer.Models
{
    public class CodeItem
    {
        public required AuthenticationRequestModel AuthenticationRequestModel { get; set; }
        public required string UserName { get; set; }
        public required string[] Scopes { get; set; }
    }
}
