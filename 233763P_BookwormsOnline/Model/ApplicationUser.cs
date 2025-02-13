using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace _233763P_BookwormsOnline.Model
{
    public class ApplicationUser: IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string CreditCard { get; set; } // Will be encrypted

        [Required]
        public string MobileNo { get; set; }

        [Required]
        public string BillingAddress { get; set; }

        [Required]
        public string ShippingAddress { get; set; }

        public string PhotoPath { get; set; } // Path to uploaded image

      
    }
}
