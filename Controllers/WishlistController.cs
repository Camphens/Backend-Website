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
    public class WishListController : Controller
    {
        private readonly WebshopContext _context;

        public WishListController(WebshopContext context)
        {
            _context = context;
        }

        [HttpPost("Move/{userid}/{productid}")]
        public IActionResult Move(int userid, int productid)
        {

            var find_cartId = (from entries in _context.Users
                               where entries.Id == userid
                               select entries.Cart.Id).ToArray();
            if (find_cartId == null)
            {
                return NotFound();
            }
            AddItemToCart(find_cartId[0], productid);
            var find_wishlist_product = (from entries in _context.WishlistProduct
                                         where entries.Wishlist.UserId == userid && entries.ProductId == productid
                                         select entries).ToArray();
            _context.WishlistProduct.Remove(find_wishlist_product[0]);
            _context.SaveChanges();
            return Ok();
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
    }
}