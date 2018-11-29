using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend_Website.Auth;
using Backend_Website.Models;
using Newtonsoft.Json;

namespace Backend_Website.Helpers
{
    public class Tokens
    {
      public static async Task<string> GenerateJwt(ClaimsIdentity identity, IJwtGenerator jwtGenerator,string userName, JwtIssuerOptions jwtOptions, JsonSerializerSettings serializerSettings)
      {
        var response = new
        {
          id = identity.Claims.Single(c => c.Type == "id").Value,
          auth_token = await jwtGenerator.GenerateEncodedToken(userName, identity),
          expires_in = (int)jwtOptions.ValidFor.TotalSeconds
        };

        return JsonConvert.SerializeObject(response, serializerSettings);
      }
    }
}