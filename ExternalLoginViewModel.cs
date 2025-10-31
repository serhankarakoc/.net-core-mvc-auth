using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels
{
    public class ExternalLoginViewModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Ad alanı gereklidir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı gereklidir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        public string ReturnUrl { get; set; }
        
        public string Provider { get; set; }
    }
}