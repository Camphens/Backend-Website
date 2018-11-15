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
    public class OrderController : Controller
    {
        private readonly WebshopContext _context;

        public OrderController(WebshopContext context)
        {
            _context = context;
        }
        // GET api/cart

        [HttpGet("GetAllOrders")]
        public ActionResult GetAllOrders()
        {
            var orders = (from items in _context.Orders
                          select items).ToList();
            return Ok(orders);

        }

        // GET api/cart/5
        [HttpGet("GetSpecificOrder/{id}")]
        public ActionResult GetSpecificOrder(int id)
        {
            var specific_order = _context.Orders.FirstOrDefault(Order => Order.Id == id);
            if (specific_order == null)
            {
                return NotFound();
            }
            //else:
            return new OkObjectResult(specific_order);
        }


        // POST api/cart
        [HttpPost("MakeOrder")]
        public void Post([FromBody] Order Order)
        {

            _context.Orders.Add(Order);
            _context.SaveChanges();
        }

        // PUT api/cart/5
        [HttpPut("UpdateOrder/{id}")]
        public ActionResult UpdateOrder(int id, [FromBody] Order UpdatedOrder)
        {
            var Old_Orderr = _context.Orders.FirstOrDefault(Order_To_Be_Updated => Order_To_Be_Updated.Id == id);
            if (Old_Orderr == null)
            {
                return NotFound();
            }
            else
            {
                Old_Orderr.Id = UpdatedOrder.Id;
                Old_Orderr.OrderTotalPrice = UpdatedOrder.OrderTotalPrice;

                _context.SaveChanges();
                return Ok(Old_Orderr);
            }

        }

        // DELETE api/cart/5
        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return Ok(order);
        }
    }
}