using System.ComponentModel.DataAnnotations;

namespace MyApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı gereklidir")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
        [StringLength(20, ErrorMessage = "Kullanıcı adı en fazla 20 karakter olabilir")]
        [Display(Name = "Kullanıcı Adı")]
        public string UserName { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin")]
        public string PhoneNumber { get; set; }
    }
}