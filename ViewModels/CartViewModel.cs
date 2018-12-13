using System;
using System.Reflection;
using Backend_Website.ViewModels.Validations;
using FluentValidation.Attributes;

namespace Backend_Website.ViewModels
{

    [Validator(typeof(CartViewModelValidator))]
    public class CartViewModel
    {
        public int ProductId            { get; set; }
        public int CartQuantity         { get; set; }
    }

    [Validator(typeof(WishlistViewModelValidator))]
    public class WishlistViewModel
    {
        public int ProductId            { get; set; }
    }

    public class WishlistToCartViewModel
    {
        public int[] ProductId          { get; set; }
    }
}