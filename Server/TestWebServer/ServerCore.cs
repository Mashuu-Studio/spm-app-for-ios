using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TestWebServer
{
    public class User
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class UserRepository
    {
        private readonly List<User> users = new List<User>()
        {
            new User{username = "ghwls", password = "ghwls" },
        };

        public User GetUser(string username, string password)
        {
            return users.Find(u => u.username == username && u.password == password);
        }
    }

    public class TokenService
    {
        private const string SecretKey = "my_very_secure_secret_key_123456";

        public string GenerateToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("username", username) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly TokenService tokenService;

        public AuthController()
        {
            userRepository = new UserRepository();
            tokenService = new TokenService();
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var authenticatedUser = userRepository.GetUser(user.username, user.password);
            if (authenticatedUser == null) return Unauthorized();

            var token = tokenService.GenerateToken(authenticatedUser.username);
            return Ok(new { Token = token });
        }
    }
}