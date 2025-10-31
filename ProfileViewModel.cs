using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MyApp.ViewModels
{
    public class ProfileViewModel
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

        [Display(Name = "Kullanıcı Adı")]
        [StringLength(20, ErrorMessage = "Kullanıcı adı en fazla 20 karakter olabilir")]
        public string UserName { get; set; }

        [Display(Name = "Telefon Numarası")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Hakkımda")]
        [StringLength(500, ErrorMessage = "Hakkımda en fazla 500 karakter olabilir")]
        public string Bio { get; set; }

        [Display(Name = "Profil Resmi")]
        public IFormFile ProfilePicture { get; set; }

        public string ProfilePictureUrl { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Konum")]
        [StringLength(100, ErrorMessage = "Konum en fazla 100 karakter olabilir")]
        public string Location { get; set; }

        [Display(Name = "Web Sitesi")]
        [Url(ErrorMessage = "Geçerli bir web sitesi URL'si girin")]
        public string Website { get; set; }
    }
}