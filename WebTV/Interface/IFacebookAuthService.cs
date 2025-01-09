namespace WebTV.Interface
{
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using WebTV.Controllers;

    public interface IFacebookAuthService
    {
        Task<FacebookUserResponse> GetUserInfoFromFacebookAsync(string accessToken);
    }
}
