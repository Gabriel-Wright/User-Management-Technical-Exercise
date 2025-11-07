using System;
using System.ComponentModel.DataAnnotations;

//Only allowing users between the age of 18 and 120
//Sorry to all the 121 year olds!!!
namespace UserManagement.Services.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BirthDateAttribute : ValidationAttribute
    {
        public int MinAge { get; set; } = 18;
        public int MaxAge { get; set; } = 120;

        public BirthDateAttribute()
        {
            ErrorMessage = $"Birth date must make the user between {MinAge} and {MaxAge} years old.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime birthDate)
            {
                return new ValidationResult("Invalid birth date.");
            }

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--; //Adjust for whether birthday has happened this year

            if (age < MinAge)
            {
                return new ValidationResult($"User must be at least {MinAge} years old.");
            }

            if (age > MaxAge)
            {
                return new ValidationResult($"User cannot be older than {MaxAge} years.");
            }

            return ValidationResult.Success;
        }
    }
}
