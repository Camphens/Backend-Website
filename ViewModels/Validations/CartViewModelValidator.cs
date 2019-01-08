using FluentValidation;
 

namespace Backend_Website.ViewModels.Validations
{
    public class CartViewModelValidator : AbstractValidator<CartViewModel>
    {
        public CartViewModelValidator()
        {
            RuleFor(vm => vm.ProductId).NotEmpty().NotNull().GreaterThan(0).WithMessage("ProductId moet meegegeven worden");
            RuleFor(vm => vm.CartQuantity).GreaterThan(0);
        }
    }

    public class WishlistViewModelValidator : AbstractValidator<WishlistViewModel>
    {
        public WishlistViewModelValidator()
        {
            RuleFor(vm => vm.ProductId).NotEmpty().NotNull().GreaterThan(0).WithMessage("ProductId moet meegegeven worden");
        }
    }
}