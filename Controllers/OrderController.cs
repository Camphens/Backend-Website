using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;
        public OrderController(WebshopContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _caller = httpContextAccessor.HttpContext.User;
        }


        [HttpGet("GetAllOrders")]
        public ActionResult GetAllOrders()
        {
            var orders = (from items in _context.Orders
                          select items).ToList();
            return Ok(orders);
        }


        [HttpGet("GetOrdersOfTheUser")]
        public ActionResult GetAllOrders(int id)
        {
            var orders = (from items in _context.Orders
                          where items.UserId == id
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


        [HttpPost("MakeOrder")]
        public void MakeOrder(dynamic Orderdetails)
        {
            dynamic OrderdetailsJSON = JsonConvert.DeserializeObject(Orderdetails.ToString());
            OrderStatus Status = new OrderStatus()
            {
                OrderDescription = "Pending"
            };
            _context.OrderStatus.Add(Status);

            Order Order = new Order()
            {
                UserId = OrderdetailsJSON.userID,
                AddressId = OrderdetailsJSON.AddressID,
                OrderStatusId = Status.Id
            };
            _context.Orders.Add(Order);

            foreach (var item in OrderdetailsJSON.productIDs)
            {
                OrderProduct product = new OrderProduct()
                {
                    OrderId = Order.Id,
                    ProductId = item
                };
                _context.OrderProduct.Add(product);
            }
            _context.SaveChanges();
        }

        [Authorize(Policy = "ApiUser")]
        [HttpPost]
        public void CreateOrder(dynamic UserAddress)
        {
            dynamic AddressJson = JsonConvert.DeserializeObject(UserAddress.ToString());
            var userId = _caller.Claims.Single(c => c.Type == "id");

            var cart_given_id = (from cart in _context.Carts
                                 where cart.UserId == int.Parse(userId.Value)
                                 select cart.Id).ToArray().First();

            var returnprice = (from entries in _context.Carts
                               where entries.Id == cart_given_id
                               select entries.CartTotalPrice).ToArray().First();
                               
            var o = new Order
            {
                UserId = int.Parse(userId.Value),
                AddressId = AddressJson.AddressId,
                OrderStatusId = 1,
                OrderTotalPrice = returnprice,
                OrderDate = DateTime.Now
            };
            _context.Orders.Add(o);

            var query = (from entries in _context.CartProducts
                         where entries.CartId == cart_given_id
                         select entries).ToArray();

            foreach (var item in query)
            {
                var orderproduct = new OrderProduct
                {
                    OrderId = o.Id,
                    ProductId = item.ProductId,
                    OrderQuantity = item.CartQuantity
                };
                _context.OrderProduct.Add(orderproduct);
            }

            foreach (var item in query)
            {
                _context.CartProducts.Remove(item);
            }
            _context.SaveChanges();
        }


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

        [HttpPost("CalculatePrice/{given_cartid}")]
        public void TotalPrice(int given_cartid)
        {
            double Sum_of_cartproducts = (from cartproducts in _context.CartProducts
                                          where cartproducts.CartId == given_cartid
                                          select cartproducts.CartQuantity *
                                          cartproducts.Product.ProductPrice).Sum();
            var price = Sum_of_cartproducts;

            var search_cart = _context.Carts.Find(given_cartid);
            search_cart.CartTotalPrice = price;
            _context.SaveChanges();
        }

        
        public IActionResult RetrievePrice(int given_cartid)
        {
            var query = (from entries in _context.Carts
                         where entries.Id == given_cartid
                         select entries.CartTotalPrice).ToArray();
            return Ok(query);

        }


    }
}