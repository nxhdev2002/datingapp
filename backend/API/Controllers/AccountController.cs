namespace API.Controllers
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using API.Data;
    using API.DTOs;
    using API.Entities;
    using API.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly DataContext _ctx;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _ctx = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginData)
        {
            var user = await _ctx.Users.SingleOrDefaultAsync(x => x.Username == loginData.Username.ToLower());
            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            /// tính hash password nhập vào
            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginData.Password));

            /// kiểm tra xem password hash có trùng với pass trong database không
            for (int i = 0; i < computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return new UserDto
            {
                Username = loginData.Username,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUser>> Register(RegisterDto registerDTO)
        {
            if (await UserExists(registerDTO.Username))
            {
                return BadRequest("Username is taker");
            }

            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                Username = registerDTO.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
                PasswordSalt = hmac.Key
            };
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();

            return user;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<bool> UserExists(string username)
        {
            return await _ctx.Users.AnyAsync(x => x.Username == username.ToLower());
        }

    }
}