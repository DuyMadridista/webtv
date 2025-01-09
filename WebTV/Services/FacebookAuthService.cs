using Newtonsoft.Json;
using WebTV.Controllers;
using WebTV.Interface;

namespace WebTV.Services
{
    public class FacebookAuthService : IFacebookAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FacebookAuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<FacebookUserResponse> GetUserInfoFromFacebookAsync(string accessToken)
        {
            var response = await _httpClient.GetAsync(
                $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to get user info from Facebook");

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookUserResponse>(content);
        }

    }
}
