using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend_Website.Models;
using Microsoft.Extensions.Options;
 

namespace Backend_Website.Auth
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly JwtIssuerOptions _jwtOptions;
        public const string Rol = "rol";

        public JwtGenerator(IOptions<JwtIssuerOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public async Task<string> GenerateEncodedToken(string emailAddress, ClaimsIdentity identity)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, emailAddress),
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64),
                identity.FindFirst(Helpers.Constants.Strings.JwtClaimIdentifiers.Id),
                identity.FindFirst("rol")
            };

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer:     _jwtOptions.Issuer,
                audience:   _jwtOptions.Audience,
                claims:     claims,
                notBefore:  _jwtOptions.NotBefore,
                expires:    _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// Generates a Claim Identity for a JWT
        public ClaimsIdentity GenerateClaimsIdentity(string emailAddress, string id, string rol)
        {
            return new ClaimsIdentity(new GenericIdentity(emailAddress, "Token"), new[]{
                new Claim(Helpers.Constants.Strings.JwtClaimIdentifiers.Id, id),
                new Claim(Rol, rol)});
        }

        /// Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero){
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));}

            if (options.SigningCredentials == null){
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));}

            if (options.JtiGenerator == null){
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));}
        }
    }
}