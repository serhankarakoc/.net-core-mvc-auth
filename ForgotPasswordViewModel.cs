using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        public bool EmailSent { get; set; }
    }
}