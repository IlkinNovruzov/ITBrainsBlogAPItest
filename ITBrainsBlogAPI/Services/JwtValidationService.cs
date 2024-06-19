//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace ITBrainsBlogAPI.Services
//{
//    public class JwtValidationService
//    {
//        private readonly IConfiguration _configuration;

//        public JwtValidationService(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public ClaimsPrincipal ValidateToken(string token)
//        {
//            var tokenHandler = new JwtSecurityTokenHandler();
//            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

//            try
//            {
//                tokenHandler.ValidateToken(token, new TokenValidationParameters
//                {
//                    ValidateIssuer = true,
//                    ValidateAudience = true,
//                    ValidateLifetime = true,
//                    ValidateIssuerSigningKey = true,
//                    ValidIssuer = _configuration["Jwt:Issuer"],
//                    ValidAudience = _configuration["Jwt:Audience"],
//                    IssuerSigningKey = new SymmetricSecurityKey(key)
//                }, out SecurityToken validatedToken);

//                return new ClaimsPrincipal((JwtSecurityToken)validatedToken);
//            }
//            catch (Exception)
//            {
//                return null;
//            }
//        }
//    }

//}
