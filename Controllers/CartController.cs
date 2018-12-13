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

        public CartController(WebshopContext context)
        {
            _context = context;
        }
        // GET api/cart

        [HttpGet("GetItemsInCart/{id}")]
        public Items_in_Cart[] GetItemsInCart(int id)
        {

            var products_in_cart = (from cart in _context.Carts
                                    where cart.Id == id
                                    let cart_items =
                                    (from entry in _context.CartProducts
                                     from product in _context.Products
                                     where entry.CartId == cart.Id && entry.ProductId == product.Id
                                     select product).ToArray()
                                    let image = (from p in cart_items
                                                 from i in _context.ProductImages
                                                 where p.Id == i.ProductId
                                                 select i.ImageURL)
                                    select new Items_in_Cart() { Cart = cart, AllItems = cart_items, Image = image }
                                   ).ToArray();

            return products_in_cart;
        }
        public class Items_in_Cart
        {
            public Cart Cart { get; set; }
            public Product[] AllItems { get; set; }

            public IEnumerable<string> Image { get; set; }
        }

        [HttpPut("ChangeQuantity")]
        public ActionResult ProductStock_GoUp(int id)
        {
            var query = (from products in _context.Products
                         where products.Id == id
                         select products.Stock).ToArray();
            query[0].ProductQuantity++;

             _context.SaveChanges();
            return Ok(query);
        }

        [HttpPut("ChangeQuantity")]
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

        [HttpPost("TotalPrice/{given_cartid}")]
        public void TotalPrice(int given_cartid)
        {
            double? Sum_of_cartproducts = (from cartproducts in _context.CartProducts
                                           where cartproducts.CartId == given_cartid
                                           select (int?)cartproducts.CartQuantity *
                                           cartproducts.Product.ProductPrice).Sum();
            var price = Sum_of_cartproducts;

            var search_cart = _context.Carts.Find(given_cartid);
            search_cart.CartTotalPrice = price;
            _context.SaveChanges();
        }
    }
}