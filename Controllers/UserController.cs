using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend_Website.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Backend_Website.ViewModels;
using System.Reflection;
using Backend_Website.ViewModels.Validations;
using Backend_Website.Helpers;

namespace Backend_Website.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;
        public UserController(WebshopContext context, IHttpContextAccessor httpContextAccessor){
            _context    = context;
            _caller     = httpContextAccessor.HttpContext.User;}
        

        [AllowAnonymous]
        [HttpPost("Registration")]
        public async Task<IActionResult> RegisterUser(dynamic UserDetails){
            dynamic UserDetailsJson = JsonConvert.DeserializeObject(UserDetails.ToString());

            User user = new User(){
                Id              = _context.Users.Select(a => a.Id).Max() + 1,
                UserPassword    = UserDetailsJson.UserPassword,
                FirstName       = UserDetailsJson.FirstName,
                LastName        = UserDetailsJson.LastName,
                BirthDate       = UserDetailsJson.BirthDate,
                Gender          = UserDetailsJson.Gender,
                EmailAddress    = UserDetailsJson.EmailAddress,
                PhoneNumber     = UserDetailsJson.PhoneNumber};

            var isvalid = Utils.IsValidAsync((UserDetailsJson.EmailAddress).ToString());
            isvalid.Wait();

            if (!isvalid.Result){
               return new BadRequestObjectResult("Onjuiste Email");}
            
            await _context.Users.AddAsync(user);
            
            Cart usercart = new Cart(){
                UserId          = user.Id, 
                CartTotalPrice  = 0.00};
            _context.Carts.Add(usercart);
        
            Wishlist userwishlist = new Wishlist(){
                UserId          = user.Id};
            _context.Wishlists.Add(userwishlist);

            Address addressDetails = new Address(){
                Id          = _context.Addresses.Select(a => a.Id).Max() + 1,
                Street      = UserDetailsJson.Street,
                City        = UserDetailsJson.City,
                ZipCode     = UserDetailsJson.ZipCode,
                HouseNumber = UserDetailsJson.HouseNumber
            };
            _context.Addresses.Add(addressDetails);

            UserAddress addressUser = new UserAddress(){
                UserId      = user.Id,
                AddressId   = addressDetails.Id
            };
            _context.UserAddress.Add(addressUser);

            await _context.SaveChangesAsync();
            return new OkObjectResult("Registratie Voltooid"); 
        }

        [HttpGet("User")]
        public ActionResult UserInfo()
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var UserInfo = (from u in _context.Users
                            where int.Parse(userId.Value) == u.Id
                            select new {u.EmailAddress, u.FirstName, u.LastName, u.BirthDate, u.Gender, u.PhoneNumber} ).ToArray(); 
            return Ok(UserInfo);
        }

        [HttpPut("User")]
        public ActionResult EditUserInfo([FromBody] UserDetailsViewModel userDetails)
        {
            UserDetailsViewModelValidator validator             = new UserDetailsViewModelValidator();
            FluentValidation.Results.ValidationResult results   = validator.Validate(userDetails);

            foreach(var failure in results.Errors)
            {
                Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
            }

            if(!ModelState.IsValid){
                return BadRequest();
            }

            var userId      = _caller.Claims.Single(c => c.Type == "id");
            var userInfo    = _context.Users.Find(int.Parse(userId.Value));
            Type type       = typeof(UserDetailsViewModel);
            Task<bool> isvalid;
            int count = 0;
            
            for(var element = 0; element < type.GetProperties().Count() - 1; element++){
                string propertyName = type.GetProperties().ElementAt(element).Name;
            
                if(userDetails[propertyName] != null)
                {
                    if(userDetails[propertyName].ToString() != "")
                    {
                        bool isnull = _context.Users.Where(b => int.Parse(userId.Value) == b.Id && b[propertyName] == null).Select(a => a).ToArray().Length == 1 ? true : false;
                        if(isnull || userDetails[propertyName].ToString() != _context.Users.Where(b => int.Parse(userId.Value) == b.Id).Select(a => a[propertyName]).ToArray()[0].ToString())
                        {
                            if(propertyName == "EmailAddress"){
                                isvalid = Utils.IsValidAsync(userDetails[propertyName].ToString());

                                if(isvalid.Result)
                                {
                                    userInfo[propertyName] = userDetails[propertyName];
                                    //Console.WriteLine("\nPropery Value: {0}", userInfo[propertyName]);
                                }
                            }
                            
                            else
                            {
                                userInfo[propertyName] = userDetails[propertyName];
                                //Console.WriteLine("\nPropery Value: {0}", userInfo[propertyName]);
                            }
                            
                            count++;
                            //Console.WriteLine("Count: {0}", count);
                            _context.Users.Update(userInfo);
                        }
                    }
                }
            };
            _context.SaveChanges();
            return Ok(ModelState);
        }

    }

}