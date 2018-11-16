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

        [HttpGet("GetItemsInCart")]
        public Items_in_Cart[] GetItemsInCart()
        {
            var products_in_cart = (from cart in _context.Carts
                                   let cart_items =
                                   (from entry in _context.CartProducts
                                    from product in _context.Products
                                    where entry.CartId == cart.Id && entry.ProductId == product.Id
                                        let img_url = 
                                        (from images in _context.ProductImages
                                        where entry.ProductId == images.ProductId
                                        select images).ToArray()
                                    select product).ToArray()
                                    let image = (from p in cart_items from i in _context.ProductImages where p.Id == i.ProductId select i.ImageURL)
                                    select new Items_in_Cart(){ Cart = cart, AllItems = cart_items, Image = image }
                                   ).ToArray();

            return products_in_cart;
        }
        public class Items_in_Cart
        {
            public Cart Cart { get; set; }
            public Product[] AllItems { get; set; }

            public IEnumerable<string> Image {get;set;}
        }


        // POST api/cart
        [HttpPost("MakeACart")]
        public void Post([FromBody] Cart Cart)
        {
            //moet nog afgemaakt worden
            _context.Carts.Add(Cart);
            _context.SaveChanges();
        }


        // DELETE api/cart/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var product_in_cart = _context.CartProducts.Find(id);
            if (product_in_cart == null)
            {
                return NotFound();
            }
            _context.CartProducts.Remove(product_in_cart);
            _context.SaveChanges();
            return Ok(product_in_cart);
        }
    }
}