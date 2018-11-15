using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly WebshopContext _context;
        
        public CartController(WebshopContext context){
            _context = context;
        }
        // GET api/cart
        
        [HttpGet("GetItemsInCart")]
        public ActionResult GetItemsInCart()
        {
           var something = (from products in _context.Products
                           select products).ToList();
            return Ok(something);
           
        }

        // GET api/cart/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/cart
        [HttpPost("MakeACart")]
        public void Post([FromBody] Cart Cart1)
        {
            
            _context.Carts.Add(Cart1);
            _context.SaveChanges();
        }

        [HttpPost("MakeAUser")]
        public void Post5([FromBody] User User1)
        {
            _context.Users.Add(User1);
            _context.SaveChanges();
        }
        

        // PUT api/cart/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/cart/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}