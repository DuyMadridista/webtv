namespace WebTV.Interface
{
    public interface IAuthService
    {
        Task<string> Authenticate(string username, string password);
        Task<string> AuthenticateWithFacebook(string accessToken);
    }
}
