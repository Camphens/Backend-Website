using FluentValidation;
using Backend_Website.Helpers;
 

namespace Backend_Website.ViewModels.Validations
{
    public class UserRegistrationViewModelValidator : AbstractValidator<UserRegistrationViewModel>
    {
        public UserRegistrationViewModelValidator()
        {
            RuleFor(a => a.BirthDate).GreaterThanOrEqualTo(new System.DateTime(1860, 1, 1, 0, 0, 0)).WithMessage("Geboortedatum onjuist");
            RuleFor(a => a.BirthDate).LessThanOrEqualTo(new System.DateTime(2019, 1, 1, 0, 0, 0)).WithMessage("Geboortedatum onjuist");
            
            RuleFor(a => a.UserPassword).MinimumLength(3).WithMessage("Wachtwoord moet meer dan 3 tekens bevatten");
        }
    }
}