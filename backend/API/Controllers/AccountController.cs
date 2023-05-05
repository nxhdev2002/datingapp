namespace API.Controllers
{
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using API.Data;
    using API.DTOs;
    using API.Entities;
    using API.Interfaces;
    using AutoMapper;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : BaseApiController
    {
        private readonly DataContext _ctx;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _tokenService = tokenService;
            _ctx = context;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginData)
        {
            var user = await _ctx.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.Username == loginData.Username.ToLower());
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
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.isMain)?.Url
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDTO)
        {
            if (await UserExists(registerDTO.Username))
            {
                return BadRequest("Username is taken");
            }

            var user = _mapper.Map<AppUser>(registerDTO);

            using var hmac = new HMACSHA512();
            user.Username = registerDTO.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password));
            user.PasswordSalt = hmac.Key;
            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();

            var token = _tokenService.CreateToken(user);
            var photoUrl = user.Photos?.FirstOrDefault(x => x.isMain)?.Url;

            return new UserDto
            {
                Username = user.Username,
                Token = token,
                PhotoUrl = photoUrl,
                KnownAs = user.KnownAs
            };
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<bool> UserExists(string username)
        {
            return await _ctx.Users.AnyAsync(x => x.Username == username.ToLower());
        }

    }
}