using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Backend_Website.Auth;
using Backend_Website.Helpers;
using Backend_Website.Models;
using Backend_Website.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly WebshopContext _context;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly JwtIssuerOptions _jwtOptions;

        public AuthController(WebshopContext context, IJwtGenerator jwtGenerator, IOptions<JwtIssuerOptions> jwtOptions)
        {
            _context = context;
            _jwtGenerator = jwtGenerator;
            _jwtOptions = jwtOptions.Value;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Post([FromBody]CredentialsViewModel credentials)
        {
            if (!ModelState.IsValid){
                return BadRequest(ModelState);}

            // Checks if Username Password combination is correct.
            var identity = await GetClaimsIdentity(credentials.EmailAddress, credentials.UserPassword);
            if (identity == null){
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));}

            // Generates Token
            var jwt = await Tokens.GenerateJwt(identity, _jwtGenerator, credentials.EmailAddress, _jwtOptions, new JsonSerializerSettings { Formatting = Formatting.Indented });
            return new OkObjectResult(jwt);
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string emailAddress, string password)
        {
            // Checks if one of the fields is null to begin with
            if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // Get the user to verifty
            var userToVerify = (from user in _context.Users
                                where user.EmailAddress == emailAddress
                                where user.UserPassword == password
                                select Tuple.Create(user.Id, user.Role)).ToArray();
            
            if(userToVerify != null)
            return await Task.FromResult(_jwtGenerator.GenerateClaimsIdentity(emailAddress, (userToVerify[0].Item1).ToString(), userToVerify[0].Item2));

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }
    }
}