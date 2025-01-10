using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebTV.Data;
using WebTV.Interface;
using WebTV.Models;

namespace WebTV.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly IFacebookAuthService _facebookAuthService;

        public AuthService(IConfiguration configuration, ApplicationDbContext context, IFacebookAuthService facebookAuthService)
        {
            _configuration = configuration;
            _context = context;
            _facebookAuthService = facebookAuthService;
        }
        public async Task<string> AuthenticateWithFacebook(string accessToken)
        {
            var facebookUser = await _facebookAuthService.GetUserInfoFromFacebookAsync(accessToken);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email!= null && u.Email == facebookUser.Email);

            if (user == null)
            {
                user = new Models.User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = facebookUser.Email ?? "duylv12@gmail.com",
                    Name = facebookUser.Name,
                    Role = "User",
                    Password = BCrypt.Net.BCrypt.HashPassword("halamadrid"),
                    FacebookId = facebookUser.Id
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            return GenerateJwtToken(user);
        }

        public async Task<string> Authenticate(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !VerifyPasswordHash(password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            return GenerateJwtToken(user);
        }

        public async Task<string> AuthenticateWithGoogle(string idToken)
        {
            try
            {
                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };

                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (existingUser == null)
                {
                    // Tạo user mới nếu chưa tồn tại
                    var newUser = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = payload.Email,
                        Name = payload.Name,
                        Role = "User" // Set role mặc định
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    existingUser = newUser;
                }

                return GenerateJwtToken(existingUser);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to authenticate with Google", ex);
            }
        }
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, user.Name)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }

}
