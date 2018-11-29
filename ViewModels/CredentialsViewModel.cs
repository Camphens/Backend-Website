using Backend_Website.ViewModels.Validations;
using FluentValidation.Attributes;

namespace Backend_Website.ViewModels
{
    [Validator(typeof(CredentialsViewModelValidator))]
    public class CredentialsViewModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}