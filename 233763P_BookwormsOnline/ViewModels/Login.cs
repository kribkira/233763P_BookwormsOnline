using System.ComponentModel.DataAnnotations;

namespace _233763P_BookwormsOnline.ViewModels
{
    public class Login
    {
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
