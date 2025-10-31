using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool EmailSent { get; set; }
        
        public string ReturnUrl { get; set; }
        
        [Display(Name = "Doğrulama Kodu")]
        public string VerificationCode { get; set; }
    }
}