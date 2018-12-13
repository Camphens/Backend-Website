using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend_Website.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Net.Mail;
using DnsClient;
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
            _context = context;
            _caller = httpContextAccessor.HttpContext.User;}
        

        [AllowAnonymous]
        [HttpPost("Registration")]
        public async Task<IActionResult> RegisterUser(dynamic UserDetails){
            dynamic UserDetailsJson = JsonConvert.DeserializeObject(UserDetails.ToString());

            User user = new User(){
                UserPassword    = UserDetailsJson.UserPassword,
                FirstName       = UserDetailsJson.FirstName,
                LastName        = UserDetailsJson.LastName,
                BirthDate       = UserDetailsJson.BirthDate,
                Gender          = UserDetailsJson.Gender,
                EmailAddress    = UserDetailsJson.EmailAddress,
                PhoneNumber     = UserDetailsJson.PhoneNumber};

            var isvalid = IsValidAsync((UserDetailsJson.EmailAddress).ToString());
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

            if(!ModelState.IsValid){
                return BadRequest();
            }

            foreach(var failure in results.Errors){
                    Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
                }

            var userId      = _caller.Claims.Single(c => c.Type == "id");
            var userInfo    = _context.Users.Find(int.Parse(userId.Value));
            Type type       = typeof(UserDetailsViewModel);
            Task<bool> isvalid;
            int count = 0;
            
            for(var element = 0; element < type.GetProperties().Count() - 1; element++){
                string propertyName = type.GetProperties().ElementAt(element).Name;
            
                if(userDetails[propertyName] != null && userDetails[propertyName].ToString() != "" && userDetails[propertyName].ToString() != _context.Users.Where(b => int.Parse(userId.Value) == b.Id).Select(a => a[propertyName]).ToArray()[0].ToString()){
                    if(propertyName == "EmailAddress"){
                        isvalid = IsValidAsync(userDetails[propertyName].ToString());

                        if(isvalid.Result){
                            userInfo[propertyName] = userDetails[propertyName];
                            Console.WriteLine("\nPropery Value: {0}", userInfo[propertyName]);}
                    }
                    
                    else{
                        userInfo[propertyName] = userDetails[propertyName];
                        Console.WriteLine("\nPropery Value: {0}", userInfo[propertyName]);}
                    
                    count++;
                    Console.WriteLine("Count: {0}", count);
                    _context.Users.Update(userInfo);
                }
            };
            _context.SaveChanges();
            return Ok(ModelState);
        }




        Task<bool> IsValidAsync(string email)
        {
            try {
                var mailAddress = new MailAddress(email);
                var host        = mailAddress.Host;
                return CheckDnsEntriesAsync(host);}
            
            catch (FormatException) {
                return Task.FromResult(false);}
        }

        async Task<bool> CheckDnsEntriesAsync(string domain)
        {
            try {
                var lookup      = new LookupClient();
                lookup.Timeout  = TimeSpan.FromSeconds(5);
                var result      = await lookup.QueryAsync(domain, QueryType.ANY).ConfigureAwait(false);

                var records = result.Answers.Where(record => record.RecordType == DnsClient.Protocol.ResourceRecordType.A || 
                                                            record.RecordType == DnsClient.Protocol.ResourceRecordType.AAAA || 
                                                            record.RecordType == DnsClient.Protocol.ResourceRecordType.MX);
                return records.Any();}

            catch (DnsResponseException) {
                return false; }
        }
    }

}