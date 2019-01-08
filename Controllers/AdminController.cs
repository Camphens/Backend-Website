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
    [Authorize(Policy = "_IsAdmin")]
    [Route("[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly WebshopContext _context;
        private readonly ClaimsPrincipal _caller;

        public AdminController(WebshopContext context, IHttpContextAccessor httpContextAccessor)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _context = context;
        }

        /////////////////////////////////////////////////////////////////////////////////
        // Cart
        /////////////////////////////////////////////////////////////////////////////////

        [HttpGet("UserId={Id}/Cart")]
        public ActionResult AdminGetCartItems(int Id)
        {
            if (_context.Users.Find(Id) != null)
            {
                var cartInfo = (from cart in _context.Carts
                                where cart.UserId == Id
                                let cart_items = from entry in _context.CartProducts
                                                 where cart.Id == entry.CartId
                                                 select new
                                                 {
                                                     product = new
                                                     {
                                                         id = entry.Product.Id,
                                                         productNumber = entry.Product.ProductNumber,
                                                         productName = entry.Product.ProductName,
                                                         productEAN = entry.Product.ProductEAN,
                                                         productInfo = entry.Product.ProductInfo,
                                                         productDescription = entry.Product.ProductDescription,
                                                         productSpecification = entry.Product.ProductSpecification,
                                                         ProductPrice = entry.Product.ProductPrice,
                                                         productColor = entry.Product.ProductColor,
                                                         Images = entry.Product.ProductImages.OrderBy(i => i.ImageURL).FirstOrDefault().ImageURL,
                                                         Type = entry.Product._Type._TypeName,
                                                         Category = entry.Product.Category.CategoryName,
                                                         Collection = entry.Product.Collection.CollectionName,
                                                         Brand = entry.Product.Brand.BrandName,
                                                         Stock = entry.Product.Stock.ProductQuantity,
                                                         itemsInCart = entry.CartQuantity
                                                     }
                                                 }
                                let cartTotal = (from item in _context.Carts
                                                 where cart.Id == item.Id
                                                 select item.CartTotalPrice)

                                select new { Products = cart_items, TotalPrice = cartTotal }).ToArray();

                return Ok(cartInfo[0]);
            }
            return NotFound();
        }

        [HttpPut("UserId={Id}/Cart")]
        public ActionResult AdminEditCartItems([FromBody] CartViewModel _cartItem, int Id)
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

            if (!ModelState.IsValid || _cartItem.CartQuantity == 0)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Find(Id) != null)
            {
                var cartItem = _context.CartProducts
                                    .Where(c => c.Cart.UserId == Id && c.ProductId == _cartItem.ProductId)
                                    .Select(a => a).ToArray();

                if (cartItem.Length == 0)
                {
                    return NotFound();
                }

                var oldQuantity = cartItem[0].CartQuantity;
                var stockid = (_context.Stock.Where(s => s.Product.Id == _cartItem.ProductId).Select(p => p.Id)).ToArray().First();
                var stock = _context.Stock.Find(stockid);

                if (stock.ProductQuantity + oldQuantity < _cartItem.CartQuantity)
                {
                    cartItem[0].CartQuantity = stock.ProductQuantity + oldQuantity;
                    stock.ProductQuantity = 0;
                }

                else
                {
                    cartItem[0].CartQuantity = _cartItem.CartQuantity;
                    stock.ProductQuantity = stock.ProductQuantity + oldQuantity - _cartItem.CartQuantity;
                }

                TotalPrice(cartItem[0].CartId);

                _context.Update(stock);
                _context.CartProducts.Update(cartItem[0]);
                _context.SaveChanges();

                return Ok();
            }
            return NotFound();
        }

        [HttpPost("UserId={Id}/Cart")]
        public ActionResult AdminPostCartItems([FromBody] CartViewModel _cartItem, int Id)
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

            if (!ModelState.IsValid || _cartItem.CartQuantity == 0)
            {
                return BadRequest(ModelState);
            }

            if (_context.Users.Find(Id) != null)
            {
                var cartId = (from cart in _context.Carts
                              where cart.UserId == Id
                              select cart.Id).ToArray();

                var stockid = (_context.Stock.Where(s => s.Product.Id == _cartItem.ProductId).Select(p => p.Id)).ToArray().First();
                var stock = _context.Stock.Find(stockid);
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

                TotalPrice(cartId[0]);

                _context.Add(product);
                _context.Stock.Update(stock);
                _context.SaveChanges();

                return Ok();
            }
            return NotFound();
        }

        [HttpDelete("UserId={Id}/Cart")]
        public ActionResult AdminDeleteCartItems([FromBody] CartViewModel _cartItem, int Id)
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

            if (_context.Users.Find(Id) != null)
            {
                var cartItem = (from item in _context.CartProducts
                                where item.Cart.UserId == Id && item.ProductId == _cartItem.ProductId
                                select item).ToArray();

                if (cartItem.Length == 0)
                {
                    return NotFound();
                }

                var stockid = (_context.Stock.Where(s => s.Product.Id == _cartItem.ProductId).Select(p => p.Id)).ToArray().First();
                var stock = _context.Stock.Find(stockid);
                stock.ProductQuantity = stock.ProductQuantity + cartItem[0].CartQuantity;
                var cartId = (from item in _context.Users
                              where item.Id == Id
                              select item.Cart.Id).ToArray();

                _context.Stock.Update(stock);
                _context.CartProducts.Remove(cartItem[0]);
                TotalPrice(cartId[0]);
                _context.SaveChanges();

                return Ok();
            }
            return NotFound();
        }

        public void TotalPrice(int given_cartid)
        {
            var Sum_of_cartproducts = (from cartproducts in _context.CartProducts
                                       where cartproducts.CartId == given_cartid
                                       select cartproducts.CartQuantity *
                                       cartproducts.Product.ProductPrice).Sum();

            var search_cart = _context.Carts.Find(given_cartid);
            search_cart.CartTotalPrice = Sum_of_cartproducts;
            _context.SaveChanges();
        }
    

        /////////////////////////////////////////////////////////////////////////////////
        // Wishlist
        /////////////////////////////////////////////////////////////////////////////////

        [HttpGet("UserId={Id}/Wishlist")]
        public ActionResult AdminGetWishlistItems(int Id)
        {
            if (_context.Users.Find(Id) != null)
            {
                var cartInfo = (from wishlist in _context.Wishlists
                                where wishlist.UserId == Id
                                let cart_items = from entry in _context.WishlistProduct
                                                where wishlist.Id == entry.WishlistId
                                                select new
                                                {
                                                    product = new
                                                    {
                                                        id = entry.Product.Id,
                                                        productNumber = entry.Product.ProductNumber,
                                                        productName = entry.Product.ProductName,
                                                        productEAN = entry.Product.ProductEAN,
                                                        productInfo = entry.Product.ProductInfo,
                                                        productDescription = entry.Product.ProductDescription,
                                                        productSpecification = entry.Product.ProductSpecification,
                                                        ProductPrice = entry.Product.ProductPrice,
                                                        productColor = entry.Product.ProductColor,
                                                        Images = entry.Product.ProductImages.OrderBy(i => i.ImageURL).FirstOrDefault().ImageURL,
                                                        Type = entry.Product._Type._TypeName,
                                                        Category = entry.Product.Category.CategoryName,
                                                        Collection = entry.Product.Collection.CollectionName,
                                                        Brand = entry.Product.Brand.BrandName,
                                                        Stock = entry.Product.Stock.ProductQuantity
                                                    }
                                                }
                                select new { Products = cart_items }).ToArray();

                return Ok(cartInfo[0]);
            }
            return NotFound();
        }

        [HttpPost("UserId={Id}/Wishlist")]
        public ActionResult AdminPostWishlistItems([FromBody] WishlistViewModel _wishlistItem, int Id)
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

            if (_context.Users.Find(Id) != null)
            {
            var wishlistId = (from wishlist in _context.Wishlists
                              where wishlist.UserId == Id
                              select wishlist.Id).ToArray();

            var exists = (from wl in _context.WishlistProduct
                          where wl.Wishlist.UserId == Id && wl.ProductId == _wishlistItem.ProductId
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
            return NotFound();
        }

        [HttpDelete("UserId={Id}/Wishlist")]
        public ActionResult AdminDeleteWishlistItems([FromBody] WishlistViewModel _wishlistItem, int Id)
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

            if (_context.Users.Find(Id) != null)
            {
            var wishlistItem = (from item in _context.WishlistProduct
                                where item.Wishlist.UserId == Id && item.ProductId == _wishlistItem.ProductId
                                select item).ToArray();

            if (wishlistItem.Length == 0)
            {
                return NotFound();
            }

            _context.WishlistProduct.Remove(wishlistItem[0]);
            _context.SaveChanges();

            return Ok();
            }
            return NotFound();
        }

        [HttpPost("UserId={Id}/Wishlist-to-Cart")]
        public ActionResult AdminWishlistItemsToCart([FromBody] WishlistToCartViewModel _wishlistItem, int Id)
        {
            if (_context.Users.Find(Id) != null)
            {
            var cartId = (from cart in _context.Carts
                          where cart.UserId == Id
                          select cart.Id).ToArray();


            foreach (var item in _wishlistItem.ProductId)
            {

                var cartItem = (from c in _context.CartProducts
                                where c.Cart.UserId == Id && c.ProductId == item
                                select c).ToArray();
                var wishlistItem = (from w in _context.WishlistProduct
                                    where w.Wishlist.UserId == Id && w.ProductId == item
                                    select w).ToArray();

                var stockid = (_context.Stock.Where(s => s.Product.Id == item).Select(p => p.Id)).ToArray().First();
                var stock = _context.Stock.Find(stockid);

                if (stock.ProductQuantity == 0)
                {
                    break;
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
                        ProductId = item,
                        CartQuantity = 1,
                        CartDateAdded = DateTime.Now
                    };

                    _context.Add(product);
                    _context.WishlistProduct.Remove(wishlistItem[0]);
                    _context.Stock.Update(stock);
                }
            }

            _context.SaveChanges();
            return Ok();
            }
            return NotFound();
        }


        /////////////////////////////////////////////////////////////////////////////////
        // User
        /////////////////////////////////////////////////////////////////////////////////

        [HttpGet("UserId={Id}")]
        public ActionResult AdminGetUserInfo(int Id)
        {
            if (_context.Users.Find(Id) != null)
            {
                var UserInfo = (from u in _context.Users
                                where Id == u.Id
                                select u).ToArray(); 
                return Ok(UserInfo);
            }
            return NotFound();
        }

        [HttpPut("UserId={Id}")]
        public ActionResult AdminEditUserInfo([FromBody] UserDetailsViewModel userDetails, int Id)
        {
            UserDetailsViewModelValidator validator             = new UserDetailsViewModelValidator();
            FluentValidation.Results.ValidationResult results   = validator.Validate(userDetails);

            foreach(var failure in results.Errors)
            {
                Errors.AddErrorToModelState(failure.PropertyName, failure.ErrorMessage, ModelState);
            }

            if(!ModelState.IsValid){
                return BadRequest();
            }

            if (_context.Users.Find(Id) != null)
            {
                var userInfo    = _context.Users.Find(Id);
                Type type       = typeof(UserDetailsViewModel);
                Task<bool> isvalid;
                int count = 0;
                
                for(var element = 0; element < type.GetProperties().Count() - 1; element++){
                    string propertyName = type.GetProperties().ElementAt(element).Name;
                
                    if(userDetails[propertyName] != null)
                    {
                        if(userDetails[propertyName].ToString() != "")
                        {
                            bool isnull = _context.Users.Where(b => Id == b.Id && b[propertyName] == null).Select(a => a).ToArray().Length == 1 ? true : false;
                            if(isnull || userDetails[propertyName].ToString() != _context.Users.Where(b => Id == b.Id).Select(a => a[propertyName]).ToArray()[0].ToString())
                            {
                                if(propertyName == "EmailAddress"){
                                    isvalid = Utils.IsValidAsync(userDetails[propertyName].ToString());

                                    if(isvalid.Result)
                                    {
                                        userInfo[propertyName] = userDetails[propertyName];
                                        //Console.WriteLine("\nPropery Value: {0}", userInfo[propertyName]);
                                    }
                                }
                                
                                else
                                {
                                    userInfo[propertyName] = userDetails[propertyName];
                                    //Console.WriteLine("\nPropery Value: {0}", userInfo[propertyName]);
                                }
                                
                                count++;
                                //Console.WriteLine("Count: {0}", count);
                                _context.Users.Update(userInfo);
                            }
                        }
                    }
                };
                _context.SaveChanges();
                return Ok(ModelState);
            }
            return NotFound();
        }
    
        [HttpDelete("UserId={Id}")]
        public ActionResult AdminDeleteUser(int Id)
        {
            if (_context.Users.Find(Id) != null)
            {
                var UserInfo = (from u in _context.Users
                                where Id == u.Id
                                select u).ToArray();
                _context.Remove(UserInfo[0]);
                _context.SaveChanges();
                return Ok();
            }
            return NotFound();
        }
    
        //Methode voor Get Users "Paged", zoals iemand producten zou krijgen.


        /////////////////////////////////////////////////////////////////////////////////
        // Order
        /////////////////////////////////////////////////////////////////////////////////

        [HttpGet("Orders")]
        public ActionResult AdminGetOrders()
        {       
            return Ok();
        }

        [HttpGet("UserId={Id}/Orders")]
        public ActionResult AdminGetUserOrders(int Id)
        {
            if (_context.Users.Find(Id) != null)
            {
                var orders = (from o in _context.Orders
                                where o.UserId == Id
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
                                select new {
                                    Order = new
                                    {
                                        id              = o.Id,
                                        userId          = o.UserId,
                                        userFirstName   = o.User.FirstName,
                                        userLastName    = o.User.LastName,
                                        userEmail       = o.User.EmailAddress,

                                        addressId       = o.AddressId,
                                        adressStreet    = o.Address.Street,
                                        adressCity      = o.Address.City,
                                        adressNumber    = o.Address.HouseNumber,
                                        adressZip       = o.Address.ZipCode,

                                        orderStatusId   = o.OrderStatusId,
                                        orderStatus     = o.OrderStatus.OrderDescription,

                                        orderTotalPrice = o.OrderTotalPrice,
                                        orderPayment    = o.OrderPaymentMethod,
                                        orderDate       = o.OrderDate,
                                        products        = o_i
                                    }
                                    }).ToArray();
                return Ok(orders);
            }
            return NotFound();
        }

        [HttpGet("Order={Id}")]
        public ActionResult AdminGetUserOrder(int Id)
        {
            if (_context.Orders.Find(Id) != null)
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
                                select new 
                                {
                                    Orders = new 
                                    {
                                        id              = o.Id,
                                        userId          = o.UserId,
                                        userFirstName   = o.User.FirstName,
                                        userLastName    = o.User.LastName,
                                        userEmail       = o.User.EmailAddress,

                                        addressId       = o.AddressId,
                                        adressStreet    = o.Address.Street,
                                        adressCity      = o.Address.City,
                                        adressNumber    = o.Address.HouseNumber,
                                        adressZip       = o.Address.ZipCode,

                                        orderStatusId   = o.OrderStatusId,
                                        orderStatus     = o.OrderStatus.OrderDescription,

                                        orderTotalPrice = o.OrderTotalPrice,
                                        orderPayment    = o.OrderPaymentMethod,
                                        orderDate       = o.OrderDate,
                                        products        = o_i
                                    }
                                }).ToArray();
                return Ok(orders.FirstOrDefault());
            }
            return NotFound();
        }

    }
}