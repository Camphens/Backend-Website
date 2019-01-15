using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Website.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Backend_Website.ViewModels;
using Backend_Website.ViewModels.Validations;
using FluentValidation.Results;
using Backend_Website.Helpers;

namespace Backend_Website.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;

        public CartController(WebshopContext context, IHttpContextAccessor httpContextAccessor)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _context = context;
        }

        [HttpGet("GetItemsInCart/{id}")]
        public Items_in_Cart[] GetItemsInCart(int id)
        {

            var products_in_cart = (from cart in _context.Carts
                                    where cart.Id == id
                                    let cart_items = (from entry in _context.CartProducts
                                                      from product in _context.Products
                                                      where entry.CartId == cart.Id && entry.ProductId == product.Id
                                                      select product).ToArray()
                                    let image = (from p in cart_items
                                                 from i in _context.ProductImages
                                                 where p.Id == i.ProductId
                                                 select i.ImageURL)

                                    select new Items_in_Cart() { Cart = cart, AllItems = cart_items, Image = image, totalprice = cart.CartTotalPrice }
                                   ).ToArray();

            return products_in_cart;
        }
        public class Items_in_Cart
        {
            public Cart Cart { get; set; }
            public Product[] AllItems { get; set; }

            public IEnumerable<string> Image { get; set; }
            public double totalprice { get; set; }
        }

        //[HttpPut("ChangeQuantity")]
        public ActionResult ProductStock_GoUp(int id)
        {
            var query = (from products in _context.Products
                         where products.Id == id
                         select products.Stock).ToArray();
            query[0].ProductQuantity++;

            _context.SaveChanges();
            return Ok(query);
        }

        //[HttpPut("ChangeQuantity")]
        public ActionResult ProductStock_GoDown(int id)
        {
            var query = (from products in _context.Products
                         where products.Id == id
                         select products.Stock).ToArray();
            if (query[0].ProductQuantity == 0)
            {
                query[0].ProductQuantity = query[0].ProductQuantity;
            }
            else
            {
                query[0].ProductQuantity--;
            }


            _context.SaveChanges();
            return Ok(query);
        }

        [HttpPost("AddItemToCart/{Cart_given_id}/{Given_ProductId}")]
        public void AddItemToCart(int Cart_given_id, int Given_ProductId)
        {
            var find_cart = (from carts in _context.CartProducts
                             where carts.CartId == Cart_given_id
                             select carts).ToArray();

            var search_product = find_cart.FirstOrDefault(existing_cart_product => existing_cart_product.ProductId == Given_ProductId);
            if (search_product == null)
            {
                var cartproduct = new CartProduct
                {
                    CartId = Cart_given_id,
                    ProductId = Given_ProductId,
                    CartQuantity = 1,
                    CartDateAdded = DateTime.Now

                };
                _context.Add(cartproduct);
                _context.SaveChanges();
            }
            else
            {
                search_product.CartQuantity++;
            }
            ProductStock_GoDown(Given_ProductId);
            _context.SaveChanges();
        }

        [HttpDelete("DeleteProductFromCart/{Given_CartId}/{Given_ProductId}")]
        public ActionResult DeleteProductFromCart(int Given_CartId, int Given_ProductId)
        {
            var product_in_cart = (from item in _context.CartProducts
                                   where item.ProductId == Given_ProductId && item.CartId == Given_CartId
                                   select item).ToArray();
            if (product_in_cart == null)
            {
                return NotFound();
            }
            ProductStock_GoUp(Given_ProductId);
            _context.CartProducts.Remove(product_in_cart[0]);
            _context.SaveChanges();
            return Ok(product_in_cart);
        }

        //[HttpGet("RetrievePrice/{given_cartid}")]
        public IActionResult RetrievePrice(int given_cartid)
        {
            var query = (from entries in _context.Carts
                         where entries.Id == given_cartid
                         select entries.CartTotalPrice).ToArray();
            return Ok(query);

        }


        /////////////////////////////////////////////////////////////////////////////////
        // User
        /////////////////////////////////////////////////////////////////////////////////

        [HttpGet]
        public ActionResult GetCartItems()
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");

            var cartInfo = (from cart in _context.Carts
                            where cart.UserId == int.Parse(userId.Value)
                            let cart_items = from entry in _context.CartProducts
                                             where cart.Id == entry.CartId
                                             select new
                                             {
                                                product = new
                                                {
                                                    id                     = entry.Product.Id,
                                                    productNumber          = entry.Product.ProductNumber,
                                                    productName            = entry.Product.ProductName,
                                                    productEAN             = entry.Product.ProductEAN,
                                                    productInfo            = entry.Product.ProductInfo,
                                                    productDescription     = entry.Product.ProductDescription,
                                                    productSpecification   = entry.Product.ProductSpecification,
                                                    ProductPrice           = entry.Product.ProductPrice,
                                                    productColor           = entry.Product.ProductColor,

                                                    Images                 = entry.Product.ProductImages.OrderBy(i => i.ImageURL).FirstOrDefault().ImageURL,
                                                    Type                   = entry.Product._Type._TypeName,
                                                    Category               = entry.Product.Category.CategoryName,
                                                    Collection             = entry.Product.Collection.CollectionName,
                                                    Brand                  = entry.Product.Brand.BrandName,
                                                    Stock                  = entry.Product.Stock.ProductQuantity,
                                                    itemsInCart            = entry.CartQuantity
                                                }
                                             }
                            let cartTotal = (from item in _context.Carts
                                             where cart.Id == item.Id
                                             select item.CartTotalPrice)
                            
                            select new { Products = cart_items, TotalPrice = cartTotal }).ToArray();

            return Ok(cartInfo[0]);

        }

        [HttpPut]
        public ActionResult EditCartItems([FromBody] CartViewModel _cartItem)
        {
            CartViewModelValidator validator    = new CartViewModelValidator();
            ValidationResult results            = validator.Validate(_cartItem);

            if (!results.IsValid)
            {
                foreach (var failure in results.Errors)
                {
                    Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
                }
            }

            if (!ModelState.IsValid || _cartItem.CartQuantity == 0)
            {
                return BadRequest(ModelState);
            }

            var userId = _caller.Claims.Single(c => c.Type == "id");
            var cartItem = _context.CartProducts
                                .Where(c => c.Cart.UserId == int.Parse(userId.Value) && c.ProductId == _cartItem.ProductId)
                                .Select(a => a).ToArray();

            if (cartItem.Length == 0)
            {
                return NotFound();
            }

            var oldQuantity = cartItem[0].CartQuantity;
            var stockid     = (_context.Stock.Where(s => s.Product.Id == _cartItem.ProductId).Select(p => p.Id)).ToArray().First();
            var stock       = _context.Stock.Find(stockid);

            if (stock.ProductQuantity < _cartItem.CartQuantity)
            {
                cartItem[0].CartQuantity = stock.ProductQuantity + oldQuantity;
                stock.ProductQuantity = 0;
            }

            else
            {
                cartItem[0].CartQuantity = _cartItem.CartQuantity;
                stock.ProductQuantity = stock.ProductQuantity + oldQuantity - _cartItem.CartQuantity;
            }
            

            _context.Update(stock);
            _context.CartProducts.Update(cartItem[0]);
            _context.SaveChanges();

            TotalPrice(cartItem[0].CartId);

            return Ok();
        }

        [HttpPost]
        public ActionResult PostCartItems([FromBody] CartViewModel _cartItem)
        {
            CartViewModelValidator validator    = new CartViewModelValidator();
            ValidationResult results            = validator.Validate(_cartItem);

            if (!results.IsValid)
            {
                foreach (var failure in results.Errors)
                {
                    Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
                }
            }

            if (!ModelState.IsValid || _cartItem.CartQuantity == 0)
            {
                return BadRequest(ModelState);
            }

            var userId      = _caller.Claims.Single(c => c.Type == "id");
            var cartId      = (from cart in _context.Carts
                                where cart.UserId == int.Parse(userId.Value)
                                select cart.Id).ToArray();
            var cartItem    = _context.CartProducts.Where(x => x.CartId == cartId.FirstOrDefault() && x.ProductId == _cartItem.ProductId).Select(x => x).ToArray();

            var stockid     = (_context.Stock.Where(s => s.Product.Id == _cartItem.ProductId).Select(p => p.Id)).ToArray().First();
            var stock       = _context.Stock.Find(stockid);

            Console.WriteLine("Lengte van cartItem is: {0}", cartItem.Length);

            if(cartItem.Length >= 1)
            {
                int oldQuantity = cartItem.FirstOrDefault().CartQuantity;
            
                if (stock.ProductQuantity < _cartItem.CartQuantity)
                {
                    cartItem[0].CartQuantity = stock.ProductQuantity + oldQuantity;
                    stock.ProductQuantity = 0;
                }

                else
                {
                    cartItem[0].CartQuantity = _cartItem.CartQuantity + oldQuantity;
                    stock.ProductQuantity = stock.ProductQuantity - cartItem[0].CartQuantity;
                }
                

                _context.Update(stock);
                _context.CartProducts.Update(cartItem[0]);
                _context.SaveChanges();

                TotalPrice(cartItem[0].CartId);

                return Ok();
            }

            var remainingQuantity = _cartItem.CartQuantity;


            if (stock.ProductQuantity == 0)
            {
                return Ok("Niet op vooraad");
            }

            else if (stock.ProductQuantity < _cartItem.CartQuantity)
            {
                remainingQuantity = stock.ProductQuantity;
                stock.ProductQuantity = 0;
            }

            else
            {
                stock.ProductQuantity = stock.ProductQuantity - _cartItem.CartQuantity;
            }

            
            CartProduct product = new CartProduct()
            {
                CartId = cartId[0],
                ProductId = _cartItem.ProductId,
                CartQuantity = remainingQuantity,
                CartDateAdded = DateTime.Now
            };

            _context.Add(product);
            _context.Stock.Update(stock);
            _context.SaveChanges();

            TotalPrice(cartId[0]);

            return Ok();
        }

        [HttpDelete]
        public ActionResult DeleteCartItems([FromBody] CartViewModel _cartItem)
        {
            CartViewModelValidator validator = new CartViewModelValidator();
            ValidationResult results = validator.Validate(_cartItem);

            if (!results.IsValid)
            {
                foreach (var failure in results.Errors)
                {
                    Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId      = _caller.Claims.Single(c => c.Type == "id");
            var cartItem    = (from item in _context.CartProducts
                                where item.Cart.UserId == int.Parse(userId.Value) && item.ProductId == _cartItem.ProductId
                                select item).ToArray();
            var cartId      = (from item in _context.Users
                                where item.Id == int.Parse(userId.Value)
                                select item.Cart.Id).ToArray();

            if (cartItem.Length == 0)
            {
                return NotFound();
            }

            var stockid = (_context.Stock.Where(s => s.Product.Id == _cartItem.ProductId).Select(p => p.Id)).ToArray().First();
            var stock   = _context.Stock.Find(stockid);
            stock.ProductQuantity = stock.ProductQuantity + cartItem[0].CartQuantity;
            
            _context.Stock.Update(stock);
            _context.CartProducts.Remove(cartItem[0]);
            _context.SaveChanges();

            TotalPrice(cartId[0]);

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