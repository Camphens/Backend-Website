using FluentValidation;
 

namespace Backend_Website.ViewModels.Validations
{
    public class CredentialsViewModelValidator : AbstractValidator<CredentialsViewModel>
    {
        public CredentialsViewModelValidator()
        {
            RuleFor(vm => vm.EmailAddress).NotEmpty().WithMessage("Email Address kan niet leeg zijn");
            RuleFor(vm => vm.UserPassword).NotEmpty().WithMessage("Wachtwoord kan niet leeg zijn");
            RuleFor(vm => vm.UserPassword).Length(3, 22).WithMessage("Wachtwoord message");
        }
    }
}