using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels
{
    public class TwoFactorAuthenticationViewModel
    {
        [Required(ErrorMessage = "Doğrulama kodu gereklidir")]
        [StringLength(7, ErrorMessage = "Doğrulama kodu 6 karakter olmalıdır", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Doğrulama Kodu")]
        public string Code { get; set; }

        public string SharedKey { get; set; }
        
        public string AuthenticatorUri { get; set; }
        
        public string[] RecoveryCodes { get; set; }
        
        public bool IsTwoFactorEnabled { get; set; }
        
        public bool IsMachineRemembered { get; set; }
    }
}