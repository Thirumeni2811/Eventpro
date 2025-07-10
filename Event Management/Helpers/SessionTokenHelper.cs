using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Event_Management.Helpers
{
    public static class SessionTokenHelper
    {
        public static Guid GetIdFromSession(ISession session)
        {
            var token = session.GetString("Token");
            Console.WriteLine("=====>" + token);
            if (string.IsNullOrEmpty(token))
                throw new Exception("Token not found in session");

            return JwtParser.ParseUserId(token);
        }
    }

    public static class JwtParser
    {
        public static Guid ParseUserId(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var uidClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uid");

            if (uidClaim == null || !Guid.TryParse(uidClaim.Value, out Guid userId))
                throw new Exception("Invalid or missing uid in token");

            return userId;
        }
    }
}
