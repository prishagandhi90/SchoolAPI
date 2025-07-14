using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VHEmpAPI.Interfaces;

namespace VHEmpAPI
{
    public class Auth : IJwtAuth
    {
        private readonly string key;
        private readonly IConfiguration _config;
        public Auth(string key, IConfiguration config)
        {
            this.key = key;
            this._config = config;
        }

        public string Authentication()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMonths(12),
              signingCredentials: credentials);

            // 1. Create Security Token Handler
            //var tokenHandler = new JwtSecurityTokenHandler();

            //// 2. Create Private Key to Encrypted
            //var tokenKey = Encoding.ASCII.GetBytes(key);

            ////3. Create JETdescriptor
            //var tokenDescriptor = new SecurityTokenDescriptor()
            //{
            //    Subject = new ClaimsIdentity(
            //        new Claim[]
            //        {
            //            new Claim("", "")
            //        }),
            //    Expires = DateTime.UtcNow.AddMonths(12),
            //    SigningCredentials = new SigningCredentials(
            //        new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            //};

            ////4. Create Token
            //var token = tokenHandler.CreateToken(tokenDescriptor);

            //// 5. Return Token from method
            //return tokenHandler.WriteToken(token);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //private string GenerateToken(IEnumerable<Claim> claims)
        //{
        //    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey:Secret"]));
        //    var _TokenExpiryTimeInHour = Convert.ToInt64(_configuration["JWTKey:TokenExpiryTimeInHour"]);
        //    var tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Issuer = _configuration["JWTKey:ValidIssuer"],
        //        Audience = _configuration["JWTKey:ValidAudience"],
        //        //Expires = DateTime.UtcNow.AddHours(_TokenExpiryTimeInHour),
        //        Expires = DateTime.UtcNow.AddMinutes(1),
        //        SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
        //        Subject = new ClaimsIdentity(claims)
        //    };

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);
        //}
    }
}
