using System.IdentityModel.Tokens.Jwt;

namespace API.Helpers
{
    public static class GetRol
    {
        public static string GetUserRol(HttpContext context)
        {
            var tokenS = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenS);
            return token.Claims.FirstOrDefault(C => C.Type == "Rol").Value.ToString();
        }
    }
}
