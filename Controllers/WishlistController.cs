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
    public class WishlistController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;

        public WishlistController(WebshopContext context, IHttpContextAccessor httpContextAccessor)
        {
            _caller = httpContextAccessor.HttpContext.User;
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

        ///////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////

        [HttpGet]
        public ActionResult GetWishlistItems()
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");

            var cartInfo = (from wishlist in _context.Wishlists
                            where wishlist.UserId == int.Parse(userId.Value)
                            let cart_items = from entry in _context.WishlistProduct
                                             where wishlist.Id == entry.WishlistId
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
                                                     Stock                  = entry.Product.Stock.ProductQuantity
                                                 }
                                             }
                            select new { Products = cart_items }).ToArray();

            return Ok(cartInfo[0]);
        }

        [HttpPost]
        public ActionResult PostWishlistItems([FromBody] WishlistViewModel _wishlistItem)
        {
            WishlistViewModelValidator validator = new WishlistViewModelValidator();
            ValidationResult results = validator.Validate(_wishlistItem);

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

            var userId = _caller.Claims.Single(c => c.Type == "id");
            var wishlistId = (from wishlist in _context.Wishlists
                              where wishlist.UserId == int.Parse(userId.Value)
                              select wishlist.Id).ToArray();

            var exists = (from wl in _context.WishlistProduct
                          where wl.Wishlist.UserId == int.Parse(userId.Value) && wl.ProductId == _wishlistItem.ProductId
                          select wl).ToArray();

            if (exists.Length != 0)
            {
                _context.WishlistProduct.Remove(exists[0]);
                //return Ok("Staat al in Wishlist");
            }

            else{
                WishlistProduct product = new WishlistProduct()
                {
                    WishlistId = wishlistId[0],
                    ProductId = _wishlistItem.ProductId
                };

                _context.Add(product);
            }
            
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        public ActionResult DeleteWishlistItems([FromBody] WishlistViewModel _wishlistItem)
        {
            WishlistViewModelValidator validator = new WishlistViewModelValidator();
            ValidationResult results = validator.Validate(_wishlistItem);

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

            var userId = _caller.Claims.Single(c => c.Type == "id");
            var wishlistItem = (from item in _context.WishlistProduct
                                where item.Wishlist.UserId == int.Parse(userId.Value) && item.ProductId == _wishlistItem.ProductId
                                select item).ToArray();

            if (wishlistItem.Length == 0)
            {
                return NotFound();
            }

            _context.WishlistProduct.Remove(wishlistItem[0]);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("toCart")]
        public ActionResult WishlistItemsToCart([FromBody] WishlistViewModel _wishlistItem)
        {
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var cartId = (from cart in _context.Carts
                          where cart.UserId == int.Parse(userId.Value)
                          select cart.Id).ToArray();


            // foreach (var item in _wishlistItem.ProductId)
            // {

            //     var cartItem = (from c in _context.CartProducts
            //                     where c.Cart.UserId == int.Parse(userId.Value) && c.ProductId == item
            //                     select c).ToArray();
            //     var wishlistItem = (from w in _context.WishlistProduct
            //                         where w.Wishlist.UserId == int.Parse(userId.Value) && w.ProductId == item
            //                         select w).ToArray();

            //     var stockid = (_context.Stock.Where(s => s.Product.Id == item).Select(p => p.Id)).ToArray().First();
            //     var stock = _context.Stock.Find(stockid);

            //     if (stock.ProductQuantity == 0)
            //     {
            //         break;
            //     }

            //     else if (cartItem.Length != 0)
            //     {
            //         _context.WishlistProduct.Remove(wishlistItem[0]);
            //     }

            //     else
            //     {
            //         stock.ProductQuantity--;

            //         CartProduct product = new CartProduct()
            //         {
            //             CartId = cartId[0],
            //             ProductId = item,
            //             CartQuantity = 1,
            //             CartDateAdded = DateTime.Now
            //         };

            //         _context.Add(product);
            //         _context.WishlistProduct.Remove(wishlistItem[0]);
            //         _context.Stock.Update(stock);
            //     }
            // }

            var cartItem = (from c in _context.CartProducts
                            where c.Cart.UserId == int.Parse(userId.Value) && c.ProductId == _wishlistItem.ProductId
                            select c).ToArray();
            var wishlistItem = (from w in _context.WishlistProduct
                                where w.Wishlist.UserId == int.Parse(userId.Value) && w.ProductId == _wishlistItem.ProductId
                                select w).ToArray();

            var stockid = (_context.Stock.Where(s => s.Product.Id == _wishlistItem.ProductId).Select(p => p.Id)).ToArray().First();
            var stock = _context.Stock.Find(stockid);

            if (stock.ProductQuantity == 0)
            {
                return Ok(new {wishlistItem = "Is niet op voorraad"});
            }

            else if (cartItem.Length != 0)
            {
                _context.WishlistProduct.Remove(wishlistItem[0]);
            }

            else
            {
                stock.ProductQuantity--;

                CartProduct product = new CartProduct()
                {
                    CartId = cartId[0],
                    ProductId = _wishlistItem.ProductId,
                    CartQuantity = 1,
                    CartDateAdded = DateTime.Now
                };

                _context.Add(product);
                _context.WishlistProduct.Remove(wishlistItem[0]);
                _context.Stock.Update(stock);
            }

            _context.SaveChanges();
            TotalPrice(cartId[0]);
            return Ok();
        }

        public void TotalPrice(int cartId)
        {
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