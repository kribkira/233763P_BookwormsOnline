using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class Register
{
    [RegularExpression(@"^[a-zA-Z0-9@.]+$", ErrorMessage = "Invalid characters.")]
    [Required]
    public string FirstName { get; set; }

    [RegularExpression(@"^[a-zA-Z0-9@.]+$", ErrorMessage = "Invalid characters.")]
    [Required]
    public string LastName { get; set; }

    [RegularExpression(@"^[a-zA-Z0-9@.]+$", ErrorMessage = "Invalid characters.")]
    [Required]
    [DataType(DataType.CreditCard)]
    public string CreditCard { get; set; } // Will be encrypted

    [RegularExpression(@"^[a-zA-Z0-9@.]+$", ErrorMessage = "Invalid characters.")]
    [Required]
    [Phone]
    public string MobileNo { get; set; }

    
    [Required]
    public string BillingAddress { get; set; }

    
    [Required]
    public string ShippingAddress { get; set; }


    
  
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    
    [Required]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; }

    
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }

    [Required]
    [DataType(DataType.Upload)]
    public IFormFile Photo { get; set; } // JPG Upload only
}


