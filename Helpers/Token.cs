using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Helpers
{
    public static class Token
    {
        public static Claim GetClaim(HttpContext context, string Type)
        {
            var tokenS = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            return token.Claims.FirstOrDefault(C => C.Type == Type);
        }
        public static string GetUserRol(HttpContext context)
        {
            var tokenS = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            return token.Claims.FirstOrDefault(C => C.Type == "Rol").Value.ToString();
        }
    }
}
