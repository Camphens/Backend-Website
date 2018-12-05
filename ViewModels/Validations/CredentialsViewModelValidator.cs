using FluentValidation;
 

namespace Backend_Website.ViewModels.Validations
{
    public class CredentialsViewModelValidator : AbstractValidator<CredentialsViewModel>
    {
        public CredentialsViewModelValidator()
        {
            RuleFor(vm => vm.EmailAddress).NotEmpty().WithMessage("Email Address cannot be empty");
            RuleFor(vm => vm.UserPassword).NotEmpty().WithMessage("Password cannot be empty");
            RuleFor(vm => vm.UserPassword).Length(3, 22).WithMessage("Password must be between 6 and 12 characters");
        }
    }
}