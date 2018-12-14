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

        [HttpGet]
        public ActionResult GetWishlistItems()
        {
            var userId      = _caller.Claims.Single(c => c.Type == "id");

            var cartInfo    =   (from wishlist in _context.Wishlists
                                where wishlist.UserId   == int.Parse(userId.Value)
                                let cart_items      =   from entry in _context.WishlistProduct
                                                        where wishlist.Id == entry.WishlistId
                                                        select new {product =new {id                      = entry.Product.Id,
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
                                                                    Stock                   = entry.Product.Stock.ProductQuantity}}
                                select new {Products = cart_items}).ToArray();
            
            return Ok(cartInfo[0]);
        }

        [HttpPost]
        public ActionResult PostWishlistItems([FromBody] WishlistViewModel _wishlistItem)
        {
            WishlistViewModelValidator validator    = new WishlistViewModelValidator();
            ValidationResult results                = validator.Validate(_wishlistItem);

            if (!results.IsValid){
                foreach(var failure in results.Errors){
                    Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
                }
            }

            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }          

            var userId      = _caller.Claims.Single(c => c.Type == "id");
            var wishlistId  = (from wishlist in _context.Wishlists
                                where wishlist.UserId == int.Parse(userId.Value)
                                select wishlist.Id).ToArray();
            
            var exists      = (from wl in _context.WishlistProduct
                                where wl.Wishlist.UserId == int.Parse(userId.Value) && wl.ProductId == _wishlistItem.ProductId
                                select wl).ToArray();
            
            if(exists.Length != 0){
                return Ok("Staat al in Wishlist");
            }

            WishlistProduct product = new WishlistProduct(){
                WishlistId  = wishlistId[0],
                ProductId   = _wishlistItem.ProductId
            };

            _context.Add(product);
            _context.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        public ActionResult DeleteWishlistItems([FromBody] WishlistViewModel _wishlistItem)
        {
            WishlistViewModelValidator validator    = new WishlistViewModelValidator();
            ValidationResult results                = validator.Validate(_wishlistItem);

            if (!results.IsValid){
                foreach(var failure in results.Errors){
                    Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
                }
            }

            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }              

            var userId          = _caller.Claims.Single(c => c.Type == "id");
            var wishlistItem    = (from item in _context.WishlistProduct
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
        public ActionResult WishlistItemsToCart([FromBody] WishlistToCartViewModel _wishlistItem)
        {
            var userId      = _caller.Claims.Single(c => c.Type == "id");
            var cartId      = (from cart in _context.Carts
                                where cart.UserId == int.Parse(userId.Value)
                                select cart.Id).ToArray();


            foreach(var item in _wishlistItem.ProductId){

                var cartItem    = (from c in _context.CartProducts
                                   where c.Cart.UserId == int.Parse(userId.Value) && c.ProductId == item
                                   select c).ToArray();
                var wishlistItem = (from w in _context.WishlistProduct
                                   where w.Wishlist.UserId == int.Parse(userId.Value) && w.ProductId == item
                                   select w).ToArray();

                var stockid = (_context.Stock.Where(s => s.Product.Id == item).Select(p => p.Id)).ToArray().First();
                var stock   = _context.Stock.Find(stockid);

                if(stock.ProductQuantity == 0){
                    break;
                }

                else if(cartItem.Length != 0){
                    _context.WishlistProduct.Remove(wishlistItem[0]);
                }
                
                else{
                    stock.ProductQuantity --;

                    CartProduct product = new CartProduct(){
                    CartId          = cartId[0],
                    ProductId       = item,
                    CartQuantity    = 1,
                    CartDateAdded   = DateTime.Now};

                    _context.Add(product);
                    _context.WishlistProduct.Remove(wishlistItem[0]);
                    _context.Stock.Update(stock);
                }
            }
            
            _context.SaveChanges();
            return Ok();
        }

    }
}