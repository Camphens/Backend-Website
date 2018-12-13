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