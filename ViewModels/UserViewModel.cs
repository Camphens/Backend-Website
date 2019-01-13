using System;
using System.Reflection;
using Backend_Website.ViewModels.Validations;
using FluentValidation.Attributes;

namespace Backend_Website.ViewModels
{
    public class RegistrationViewModel
    {
        public string EmailAddress { get; set; }
        public string UserPassword { get; set; }
    }

    [Validator(typeof(UserDetailsViewModelValidator))]
    public class UserDetailsViewModel
    {
        public string UserPassword  { get; set; }
        public string FirstName     { get; set; }
        public string LastName      { get; set; }
        public DateTime? BirthDate  { get; set; }
        public string Gender        { get; set; }
        public string EmailAddress  { get; set; }
        public int? PhoneNumber     { get; set; }

        public object this[string propertyName] {
        get{
           Type myType = typeof(UserDetailsViewModel);                   
           PropertyInfo myPropInfo = myType.GetProperty(propertyName);
           return myPropInfo.GetValue(this, null);}
        set{
           Type myType = typeof(UserDetailsViewModel);                   
           PropertyInfo myPropInfo = myType.GetProperty(propertyName);
           myPropInfo.SetValue(this, value, null);}
        }
    }

    public class UserDetailsViewModelAdmin
    {
        public string UserPassword  { get; set; }
        public string FirstName     { get; set; }
        public string LastName      { get; set; }
        public DateTime? BirthDate  { get; set; }
        public string Gender        { get; set; }
        public string EmailAddress  { get; set; }
        public int? PhoneNumber     { get; set; }
        public string Street        { get; set; }
        public string City          { get; set; }
        public string ZipCode       { get; set; }
        public string HouseNumber   { get; set; }

        public object this[string propertyName] {
        get{
           Type myType = typeof(UserDetailsViewModelAdmin);                   
           PropertyInfo myPropInfo = myType.GetProperty(propertyName);
           return myPropInfo.GetValue(this, null);}
        set{
           Type myType = typeof(UserDetailsViewModelAdmin);                   
           PropertyInfo myPropInfo = myType.GetProperty(propertyName);
           myPropInfo.SetValue(this, value, null);}
        }
    }

    [Validator(typeof(UserRegistrationViewModelValidator))]
    public class UserRegistrationViewModel
    {
        public string UserPassword  { get; set; }
        public string FirstName     { get; set; }
        public string LastName      { get; set; }
        public DateTime BirthDate   { get; set; }
        public string Gender        { get; set; }
        public string EmailAddress  { get; set; }
        public int? PhoneNumber     { get; set; }
        public string Street        { get; set; }
        public string City          { get; set; }
        public string ZipCode       { get; set; }
        public string HouseNumber   { get; set; }
    }
}