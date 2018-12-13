using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend_Website.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend_Website.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;
        public UserInfoController(WebshopContext context, IHttpContextAccessor httpContextAccessor){
            _context = context;
            _caller = httpContextAccessor.HttpContext.User;}
        


        [HttpGet("User")]
        public ActionResult UserInfo()
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var UserInfo = (from u in _context.Users
                            where int.Parse(userId.Value) == u.Id
                            select u).ToArray(); 
            return Ok(UserInfo);
        }
    }
}