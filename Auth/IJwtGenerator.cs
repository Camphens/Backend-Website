using System.Security.Claims;
using System.Threading.Tasks;

namespace Backend_Website.Auth
{
    public interface IJwtGenerator
    {
        Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity);
        ClaimsIdentity GenerateClaimsIdentity(string userName, string id);
    }
}