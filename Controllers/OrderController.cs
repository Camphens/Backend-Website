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
    [Authorize(Policy = "ApiUser")]
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

        [Authorize(Policy = "ApiUser")]
        [HttpGet("GetOrdersOfTheUser")]
        public ActionResult GetAllOrders(int id)
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var orders = (from items in _context.Orders
                          where items.UserId == int.Parse(userId.Value)
                          select items).ToList();
            return Ok(orders);

        }

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

        [Authorize(Policy = "ApiUser")]
        [HttpPost("create")]
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

        [HttpPut("UpdateOrder")]
        public ActionResult UpdateOrder([FromBody] Order UpdatedOrder)
        {
            var Old_Orderr = _context.Orders.FirstOrDefault(Order_To_Be_Updated => Order_To_Be_Updated.Id == UpdatedOrder.Id);
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

        [HttpDelete("DelOrder")]
        public IActionResult DeleteOrder(dynamic id)
        {
            var OrderIdJson = JsonConvert.DeserializeObject(id.ToString());
            int orderid = OrderIdJson.AddressId;
            var order = _context.Orders.Find(orderid);
            if (order == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return Ok(order);
        }

        public IActionResult RetrievePrice(int given_cartid)
        {
            var query = (from entries in _context.Carts
                         where entries.Id == given_cartid
                         select entries.CartTotalPrice).ToArray();
            return Ok(query);

        }



        /////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public ActionResult getOrder()
        {
            var Id = int.Parse((_caller.Claims.Single(c => c.Type == "id")).Value);
            
            var orders = (from o in _context.Orders
                            where o.UserId == Id
                            let o_i = (from entry in _context.OrderProduct
                                        where entry.OrderId == o.Id
                                        orderby entry.Order.OrderDate descending
                                        select new
                                        {
                                            product = new 
                                            {
                                                id                      = entry.Product.Id,
                                                productNumber           = entry.Product.ProductNumber,
                                                productName             = entry.Product.ProductName,
                                                productEAN              = entry.Product.ProductEAN,
                                                productInfo             = entry.Product.ProductInfo,
                                                productDescription      = entry.Product.ProductDescription,
                                                productSpecification    = entry.Product.ProductSpecification,
                                                ProductPrice            = entry.Product.ProductPrice,
                                                productColor            = entry.Product.ProductColor,
                                                Images                  = entry.Product.ProductImages.OrderBy(i => i.ImageURL).FirstOrDefault().ImageURL,
                                                Type                    = entry.Product._Type._TypeName,
                                                Category                = entry.Product.Category.CategoryName,
                                                Collection              = entry.Product.Collection.CollectionName,
                                                Brand                   = entry.Product.Brand.BrandName,
                                                Stock                   = entry.Product.Stock.ProductQuantity,
                                                itemsInOrder            = entry.OrderQuantity,

                                                orderStatus             = entry.Order.OrderStatus.OrderDescription,
                                                orderPayment            = entry.Order.OrderPaymentMethod,
                                                orderDate               = entry.Order.OrderDate,
                                                orderid                 = entry.Order.Id,

                                                adressStreet            = entry.Order.Address.Street,
                                                adressCity              = entry.Order.Address.City,
                                                adressNumber            = entry.Order.Address.HouseNumber,
                                                adressZip               = entry.Order.Address.ZipCode
                                            }        
                                        })
                            select new {Products = o_i}).ToArray();
            return Ok(orders.FirstOrDefault());
        }

        [HttpGet("Specific={Id}")]
        public ActionResult getOrderwithId(int Id)
        {
            var userId = int.Parse((_caller.Claims.Single(c => c.Type == "id")).Value);
            var exists = _context.Orders.Where(x => x.Id == Id && x.UserId == userId).Select(x => x).ToArray();
            if (exists.Length == 1)
            {
                var orders = (from o in _context.Orders
                                where o.Id == Id
                                let o_i = (from entry in _context.OrderProduct
                                            where entry.OrderId == o.Id
                                            select new
                                            {
                                                product = new 
                                                {
                                                    id = entry.Product.Id,
                                                    productNumber           = entry.Product.ProductNumber,
                                                    productName             = entry.Product.ProductName,
                                                    productEAN              = entry.Product.ProductEAN,
                                                    productInfo             = entry.Product.ProductInfo,
                                                    productDescription      = entry.Product.ProductDescription,
                                                    productSpecification    = entry.Product.ProductSpecification,
                                                    ProductPrice            = entry.Product.ProductPrice,
                                                    productColor            = entry.Product.ProductColor,
                                                    Images                  = entry.Product.ProductImages.OrderBy(i => i.ImageURL).FirstOrDefault().ImageURL,
                                                    Type                    = entry.Product._Type._TypeName,
                                                    Category                = entry.Product.Category.CategoryName,
                                                    Collection              = entry.Product.Collection.CollectionName,
                                                    Brand                   = entry.Product.Brand.BrandName,
                                                    Stock                   = entry.Product.Stock.ProductQuantity,
                                                    itemsInOrder            = entry.OrderQuantity
                                                }        
                                            })
                                select new { Products = o_i }).ToArray();
                return Ok(orders.FirstOrDefault());
            }
            return NotFound();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Order(dynamic OrderDetailsJson)
        {
            dynamic OrderDetails = JsonConvert.DeserializeObject(OrderDetailsJson.ToString());

            int?    userId;
            int     addressId;
            double  totalPrice;
            string  orderPaymentMethod = OrderDetails.OrderPaymentMethod;
            int     availableOrderId = 1;

            if(_context.Orders.Count() != 0){
                availableOrderId = _context.Orders.Select(a => a.Id).Max() + 1;
            }

            if(_caller.HasClaim(claim => claim.Type == "id"))
            {
                userId = int.Parse((_caller.Claims.Single(claim => claim.Type == "id")).Value);

                addressId = (from item in _context.UserAddress
                            where item.UserId == userId
                            select item.AddressId).ToArray().FirstOrDefault();

                var cartItems   = (from cart in _context.Carts
                                    where cart.UserId   == userId
                                    let cartentry       =   (from entries in _context.CartProducts
                                                            where entries.CartId == cart.Id
                                                            select entries)
                                    select cartentry).ToArray().First();

                totalPrice      = (from cart in _context.Carts
                                    where cart.UserId   == userId
                                    select cart.CartTotalPrice).ToArray().First();
                
                if (OrderDetails.Discount >= 0)
                {
                    double discounter = OrderDetails.Discount;
                    totalPrice = totalPrice - (totalPrice * (discounter/100));
                }

                var o = new Order
                {
                    Id              = availableOrderId,
                    UserId          = userId,
                    AddressId       = addressId,
                    OrderStatusId   = 1,
                    OrderTotalPrice = totalPrice,
                    OrderPaymentMethod = orderPaymentMethod,
                    OrderDate       = DateTime.Now
                };
                _context.Orders.Add(o);

                foreach (var item in cartItems)
                {
                    int productItem = item.ProductId;
                    var productSelection = _context.OrderProduct.Where(x => x.OrderId == o.Id && x.ProductId == productItem).Select(x => x);

                    if (productSelection.Count() == 1){
                       productSelection.FirstOrDefault().OrderQuantity = productSelection.FirstOrDefault().OrderQuantity + 1;
                       _context.Update(productSelection);
                    }

                    else{
                        var orderproduct = new OrderProduct
                        {
                            OrderId         = o.Id,
                            ProductId       = item.ProductId,
                            OrderQuantity   = item.CartQuantity
                        };
                        _context.OrderProduct.Add(orderproduct);
                        _context.CartProducts.Remove(item);
                    }
                }

                _context.SaveChanges();
                TotalPrice(_context.Users.Where(x => x.Id == userId).Select(x => x.Cart.Id).FirstOrDefault());
            }

            else
            {
                userId = null;
                int availableAddressId = 1;
                if(_context.Addresses.Count() != 0){
                    availableAddressId = _context.Addresses.Select(a => a.Id).Max() + 1;
                }

                Address addressUser = new Address
                {
                    Id          = availableAddressId,
                    Street      = OrderDetails.Street,
                    City        = OrderDetails.City,
                    ZipCode     = OrderDetails.ZipCode,
                    HouseNumber = OrderDetails.HouseNumber
                };
                _context.Addresses.Add(addressUser);

                addressId       = addressUser.Id;
                totalPrice      = OrderDetails.totalPrice;
                var cartItems   = OrderDetails.cartItems;

                var o = new Order
                {
                    Id              = availableOrderId,
                    UserId          = userId,
                    AddressId       = addressId,
                    OrderStatusId   = 1,
                    OrderTotalPrice = totalPrice,
                    OrderDate       = DateTime.Now,
                    OrderPaymentMethod = orderPaymentMethod,
                };
                _context.Orders.Add(o);


                foreach (var item in cartItems)
                {
                    int productItem = item.ProductId;
                    var productSelection = _context.OrderProduct.Where(x => x.OrderId == o.Id && x.ProductId == productItem).Select(x => x);

                    if (productSelection.Count() == 1){
                       productSelection.FirstOrDefault().OrderQuantity = productSelection.FirstOrDefault().OrderQuantity + 1;
                       _context.Update(productSelection);
                    }

                    else{
                        var orderproduct = new OrderProduct
                        {
                            OrderId         = o.Id,
                            ProductId       = item.ProductId,
                            OrderQuantity   = item.CartQuantity
                        };
                        _context.OrderProduct.Add(orderproduct);
                    }
                }

                _context.SaveChanges();
            }
            
            return Ok();
        }

        public void TotalPrice(int cartId)
        {
            // var Sum_of_cartproducts = (from cartproducts in _context.CartProducts
            //                             where cartproducts.CartId == given_cartid
            //                             select cartproducts.CartQuantity *
            //                             cartproducts.Product.ProductPrice).Sum();
            
            // Console.WriteLine("this is the sum of cartproducts: " + Sum_of_cartproducts);
            // var search_cart = _context.Carts.Find(given_cartid);
            // search_cart.CartTotalPrice = Sum_of_cartproducts;
            // _context.SaveChanges();

            var totalPrice  = (from item in _context.CartProducts
                                where cartId == item.CartId
                                select (item.Product.ProductPrice * item.CartQuantity)).Sum();
            
            var cart        = _context.Carts.Find(cartId);
            cart.CartTotalPrice = totalPrice;
            _context.Carts.Update(cart);
            _context.SaveChanges();
        }
    }
}