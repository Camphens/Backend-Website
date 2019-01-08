using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;



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


        [HttpGet("GetAddress")]
        public ActionResult GetMyAddress(dynamic ad)
        {
            var AddressJson = JsonConvert.DeserializeObject(ad.ToString());
            int address_id = AddressJson.AddressId;
            var address = (from addresses in _context.UserAddress
                           where addresses.AddressId == address_id
                           select addresses.Addresses).ToList();
            return Ok(address);
        }

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
            var user_adress = new UserAddress
            {
                AddressId = filled_in_adress.Id,
                UserId = Int32.Parse(userid.Value)
            };
            ;
            _context.UserAddress.Add(user_adress);

            _context.SaveChanges();
        }

        [HttpPut("UpdateTheAddress")]
        public IActionResult Update([FromBody] Address Updated_Address)
        {
            var specific_address = _context.Addresses.FirstOrDefault(Address_To_Be_Changed => Address_To_Be_Changed.Id == Updated_Address.Id);
            if (specific_address == null)
            {
                return NotFound();
            }
            //else:
            specific_address.Street         = Updated_Address.Street;
            specific_address.City           = Updated_Address.City;
            specific_address.ZipCode        = Updated_Address.ZipCode;
            specific_address.HouseNumber    = Updated_Address.HouseNumber;
            _context.SaveChanges();

            return Ok();
        }

        [HttpDelete("DeleteAddress")]
        public IActionResult DeleteAddress(dynamic address_id_)
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var AddressJson = JsonConvert.DeserializeObject(address_id_.ToString());
            int address_id = AddressJson.AddressId;
            var adress_in_useradress = (from entry in _context.UserAddress
                                        where entry.AddressId == address_id && entry.UserId == int.Parse(userId.Value)
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
    
    


        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////
        
        [HttpGet]
        public ActionResult RetrieveAddress()
        {
            var userid  = (_caller.Claims.Single(claim => claim.Type == "id"));
            var address = (from addresses in _context.UserAddress
                           where addresses.UserId == int.Parse(userid.Value)
                           select addresses.Addresses).ToArray();

            if( address.Length == 0)
            {
                return NotFound();
            }
            return Ok(address.FirstOrDefault());
        }

        [HttpPut]
        public ActionResult EditAddress([FromBody]Address addressUpdated)
        {
            var userid  = (_caller.Claims.Single(claim => claim.Type == "id"));
            var address = (from addresses in _context.UserAddress
                           where addresses.UserId == int.Parse(userid.Value)
                           select addresses.Addresses).ToArray();
            
            if( address.Length != 0)
            {
                var addressUser = address.FirstOrDefault();

                addressUser.Street      = addressUpdated.Street;
                addressUser.City        = addressUpdated.City;
                addressUser.ZipCode     = addressUpdated.ZipCode;
                addressUser.HouseNumber = addressUpdated.HouseNumber;

                _context.SaveChanges();

                return Ok();
            }

            return NotFound();
        }
    }
}