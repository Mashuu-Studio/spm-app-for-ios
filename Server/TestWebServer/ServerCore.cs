using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TestWebServer
{
    public class User
    {
        public string userName { get; set; }
        public string password { get; set; }
    }

    public class Result
    {
        public Measure[] measurements { get; set; }
    }
    public class Measure
    {
        public int itemId { get; set; }
        public string status { get; set; }
        public int value { get; set; }
    }

    public class UserRepository
    {
        private readonly List<User> users = new List<User>()
        {
            new User{userName = "master", password = "qkrwhdwnsp2j!" },
        };

        public User GetUser(string userName, string password)
        {
            return users.Find(u => u.userName == userName && u.password == password);
        }
    }

    public class TokenService
    {
        private const string SecretKey = "my_very_secure_secret_key_123456";

        public string GenerateToken(string userName)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("userName", userName) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    [ApiController]
    [Route("v1.0/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly TokenService tokenService;

        public AuthController()
        {
            userRepository = new UserRepository();
            tokenService = new TokenService();
        }

        [HttpPost("sign-in")]
        public IActionResult Login([FromBody] User user)
        {
            var authenticatedUser = userRepository.GetUser(user.userName, user.password);
            if (authenticatedUser == null) return Unauthorized();

            var token = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJtYXN0ZXIiLCJhdXRoIjoiUk9MRV9BRE1JTiIsImlkIjoxLCJuYW1lIjoibWFzdGVyIiwiZXhwIjoxNzMxMTUxNDcyfQ.czbfi5gVXHEtaWUmYJ0OmtvFFmBud6537WfOpe5AO14";
            return Ok(new { Token = token });
        }
    }

    [Route("v1.0/ai-measurement")]
    public class MeasureController : ControllerBase
    {
        private readonly TokenService tokenService;
        private readonly string check = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJtYXN0ZXIiLCJhdXRoIjoiUk9MRV9BRE1JTiIsImlkIjoxLCJuYW1lIjoibWFzdGVyIiwiZXhwIjoxNzMxMTUxNDcyfQ.czbfi5gVXHEtaWUmYJ0OmtvFFmBud6537WfOpe5AO14";
        public MeasureController()
        {
            tokenService = new TokenService();
        }

        [HttpPost("submit")]
        public IActionResult Submit([FromBody] Result result)
        {
            var authHeader = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { Message = "Missing or invalid token" });
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (token != check || result.measurements.Length != 6) return Unauthorized();

            return Ok(new { Token = tokenService.GenerateToken("measure") });
        }
    }
}