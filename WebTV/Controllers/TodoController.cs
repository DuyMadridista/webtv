using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebTV.Data;
using WebTV.Interface;
using WebTV.Models;
using WebTV.Services;

namespace WebTV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public TodoController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // GET: api/Todo
        [HttpGet]

        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            return await _context.TodoItems.ToListAsync();
        }

        // POST: api/Todo
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem todoItem)
        {
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTodoItems), new { id = todoItem.Id }, todoItem);
        }

        // POST: api/Auth/login
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var token = await _authService.Authenticate(loginRequest.Email, loginRequest.Password);
            return Ok(new { token });
        }
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerRequest.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exists." });
            }
            var newUser = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = registerRequest.Name,
                Email = registerRequest.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Role = registerRequest.Role,
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful." });
        }

        [Route("facebook-login")]
        [HttpPost]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginRequest model)
        {
            try
            {
                var token = await _authService.AuthenticateWithFacebook(model.AccessToken);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to authenticate with Facebook", error = ex.Message });
            }
        }
    }
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; } 
        public string Role { get; set; }
    }
    public class FacebookLoginRequest
    {
        public string AccessToken { get; set; }
    }
    public class FacebookUserResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
