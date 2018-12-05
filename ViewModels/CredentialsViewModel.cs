using Backend_Website.ViewModels.Validations;
using FluentValidation.Attributes;

namespace Backend_Website.ViewModels
{
    [Validator(typeof(CredentialsViewModelValidator))]
    public class CredentialsViewModel
    {
        public string EmailAddress { get; set; }
        public string UserPassword { get; set; }
    }
}