using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Event_Management.Helpers
{
    public static class TokenHelper
    {
        public static Guid GetIdFromToken(HttpRequest request)
        {
            var token = request.Cookies["Token"];
            if (string.IsNullOrEmpty(token))
                return Guid.Empty;

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtToken;

            try
            {
                jwtToken = handler.ReadJwtToken(token);
            }
            catch
            {
                return Guid.Empty; // Invalid JWT format
            }

            var uidClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");

            if (uidClaim == null || !Guid.TryParse(uidClaim.Value, out Guid userId))
                return Guid.Empty;

            return userId;
        }
    }
}
