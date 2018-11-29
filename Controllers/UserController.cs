using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend_Website.Models;
using Newtonsoft.Json;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly WebshopContext _context;
        public UserController(WebshopContext context){
            _context = context;}
        


        [HttpPost("Registration")]
        public async Task<IActionResult> RegisterUser(dynamic UserDetails){
            dynamic UserDetailsJson = JsonConvert.DeserializeObject(UserDetails.ToString());
            
            // if (!ModelState.IsValid)
            //     return BadRequest();

            User user = new User(){
                UserName        = UserDetailsJson.UserName,
                UserPassword    = UserDetailsJson.UserPassword,
                FirstName       = UserDetailsJson.FirstName,
                LastName        = UserDetailsJson.LastName,
                BirthDate       = UserDetailsJson.BirthDate,
                Gender          = UserDetailsJson.Gender,
                EmailAddress    = UserDetailsJson.EmailAddress,
                PhoneNumber     = UserDetailsJson.PhoneNumber};
            await _context.Users.AddAsync(user);
            
            Cart usercart = new Cart(){
                UserId          = user.Id, 
                CartTotalPrice  = 0.00};
            _context.Carts.Add(usercart);
        
            Wishlist userwishlist = new Wishlist(){
                UserId          = user.Id};
            _context.Wishlists.Add(userwishlist);
            await _context.SaveChangesAsync();

            return new OkObjectResult("Registration Complete");
        }
    }
}