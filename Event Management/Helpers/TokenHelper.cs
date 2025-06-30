using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Event_Management.Helpers
{
    public static class TokenHelper
    {
        public static Guid GetIdFromToken(HttpRequest request)
        {
            var token = request.Cookies["Token"];
            if (string.IsNullOrEmpty(token))
                throw new Exception("Token not found in cookies");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var uidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uid");

            if (uidClaim == null || !Guid.TryParse(uidClaim.Value, out Guid orgId))
                throw new Exception("Invalid or missing uid in token");

            return orgId;
        }
    }
}
