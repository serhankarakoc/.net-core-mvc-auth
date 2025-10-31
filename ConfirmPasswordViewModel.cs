using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels
{
    public class ConfirmPasswordViewModel
    {
        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
        
        public string Purpose { get; set; }
    }
}