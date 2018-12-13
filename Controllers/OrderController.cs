using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Website.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : Controller
    {
        private readonly WebshopContext _context;
        public OrderController(WebshopContext context){
            _context = context;}


        [HttpGet("GetAllOrders")]
        public ActionResult GetAllOrders(){
            var orders = (from items in _context.Orders
                          select items).ToList();
            return Ok(orders);}


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
        public ActionResult GetSpecificOrder(int id){
            var specific_order = _context.Orders.FirstOrDefault(Order => Order.Id == id);
            if (specific_order == null){
                return NotFound();}
            //else:
            return new OkObjectResult(specific_order);}


        [HttpPost("MakeOrder")]
        public void MakeOrder(dynamic Orderdetails){
            dynamic OrderdetailsJSON = JsonConvert.DeserializeObject(Orderdetails.ToString());
            OrderStatus Status = new OrderStatus(){
                OrderDescription = "Pending"};
            _context.OrderStatus.Add(Status);
            
            Order Order = new Order(){
                UserId = OrderdetailsJSON.userID, 
                AddressId = OrderdetailsJSON.AddressID, 
                OrderStatusId = Status.Id};
            _context.Orders.Add(Order);

            foreach (var item in OrderdetailsJSON.productIDs){
                OrderProduct product = new OrderProduct(){
                    OrderId = Order.Id, 
                    ProductId = item};
                _context.OrderProduct.Add(product);}
            _context.SaveChanges();
        }


        [HttpPut("UpdateOrder/{id}")]
        public ActionResult UpdateOrder(int id, [FromBody] Order UpdatedOrder){
            var Old_Orderr = _context.Orders.FirstOrDefault(Order_To_Be_Updated => Order_To_Be_Updated.Id == id);
            if (Old_Orderr == null){
                return NotFound();}
            else {
                Old_Orderr.Id = UpdatedOrder.Id;
                Old_Orderr.OrderTotalPrice = UpdatedOrder.OrderTotalPrice;

                _context.SaveChanges();
                return Ok(Old_Orderr);
                }
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id){
            var order = _context.Orders.Find(id);
            if (order == null){
                return NotFound();}
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return Ok(order);
        }
    }
}