using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Backend_Website.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;

        public AddressController(WebshopContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _caller = httpContextAccessor.HttpContext.User;
        }


        [HttpGet("GetAddress/{id}")]
        public ActionResult GetMyAddress(int id)
        {
            var address = (from addresses in _context.UserAddress
                           where addresses.AddressId == id
                           select addresses.Addresses).ToList();
            return Ok(address);
        }

        // POST api/values
        [HttpPost("MakeAnAddress")]
        public void FillinAdress([FromBody]Address address)
        {
            var userid = (_caller.Claims.Single(claim => claim.Type == "id"));
            var filled_in_adress = new Address
            {
                Street = address.Street,
                City = address.City,
                ZipCode = address.ZipCode,
                HouseNumber = address.HouseNumber,
            };
            _context.Addresses.Add(filled_in_adress);
            // _context.SaveChanges();

            // var find_adress_id = (from entries in _context.Addresses
            //                       where entries.Street == address.Street && entries.City == address.City && entries.ZipCode == address.ZipCode && entries.HouseNumber == address.HouseNumber
            //                       select entries.Id).ToArray();

            var user_adress = new UserAddress
            {
                AddressId = filled_in_adress.Id,
                UserId = Int32.Parse(userid.Value)
            };
            ;
            _context.UserAddress.Add(user_adress);

            _context.SaveChanges();
        }

        // PUT api/values/5
        [HttpPut("UpdateTheAddress/{id}")]
        public IActionResult Update(int id, [FromBody] Address Updated_Address)
        {
            var specific_address = _context.Addresses.FirstOrDefault(Address_To_Be_Changed => Address_To_Be_Changed.Id == id);
            if (specific_address == null)
            {
                return NotFound();
            }
            //else:
            specific_address.Street = Updated_Address.Street;
            specific_address.City = Updated_Address.City;
            specific_address.ZipCode = Updated_Address.ZipCode;
            specific_address.HouseNumber = Updated_Address.HouseNumber;
            _context.SaveChanges();
            return Ok();
        }


        [HttpDelete("DeleteAddress/{address_id}/{user_id}")]
        public IActionResult DeleteAddress(int address_id, int user_id)
        {
            var adress_in_useradress = (from entry in _context.UserAddress
                                        where entry.AddressId == address_id && entry.UserId == user_id
                                        select entry).ToArray();
            var adress_in_address_table = _context.Addresses.Find(address_id);

            if (adress_in_useradress == null || adress_in_address_table == null)
            {
                return NotFound();
            }
            _context.UserAddress.Remove(adress_in_useradress[0]);
            _context.Addresses.Remove(adress_in_address_table);
            _context.SaveChanges();
            return Ok();
        }
    }
}