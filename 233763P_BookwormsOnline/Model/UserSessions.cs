using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _233763P_BookwormsOnline.Model
{
    public class UserSession
    {
        [Key]
        [Required]
        [MaxLength(450)] // Matches Identity User ID length
        public string UserEmail { get; set; }
      
    }
}
