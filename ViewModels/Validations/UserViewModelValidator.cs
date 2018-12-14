using FluentValidation;
 

namespace Backend_Website.ViewModels.Validations
{
    public class UserDetailsViewModelValidator : AbstractValidator<UserDetailsViewModel>
    {
        public UserDetailsViewModelValidator()
        {
            RuleFor(a => a.BirthDate).GreaterThanOrEqualTo(new System.DateTime(1900, 1, 1, 0, 0, 0)).WithMessage("Kom op nou, je kan mij niet vertellen dat je 118 bent.");
        }
    }
}