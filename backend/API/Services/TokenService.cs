using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            string configKey = config["TokenKey"];
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
        }

        public string CreateToken(AppUser user)
        {
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.NameId, user.Username)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}