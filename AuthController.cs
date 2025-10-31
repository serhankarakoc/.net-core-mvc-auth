using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Encodings.Web;
using MyApp.ViewModels;
using MyApp.Models;
using System.Security.Claims;

namespace MyApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        #region Giriş İşlemleri

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", model.Email);
                    return RedirectToLocal(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Kullanıcı hesabı kilitlendi: {Email}", model.Email);
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Kullanıcı çıkış yaptı.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        #endregion

        #region Kayıt İşlemleri

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Yeni kullanıcı oluşturuldu: {Email}", model.Email);

                    // Email doğrulama token'ı oluştur ve gönder
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Action(
                        nameof(ConfirmEmail),
                        "Auth",
                        new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "E-posta Adresinizi Doğrulayın",
                        $"Lütfen e-posta adresinizi doğrulamak için <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya tıklayın</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToAction(nameof(VerifyEmail), new { email = model.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"'{userId}' ID'li kullanıcı bulunamadı.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Kullanıcı e-postası doğrulandı: {Email}", user.Email);
                return View("ConfirmEmail");
            }
            else
            {
                _logger.LogError("E-posta doğrulama başarısız: {Email}", user.Email);
                return View("Error");
            }
        }

        #endregion

        #region E-posta Doğrulama

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyEmail(string email)
        {
            var model = new VerifyEmailViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Kullanıcı bulunamasa bile güvenlik nedeniyle başarılı gibi göster
                    model.EmailSent = true;
                    return View("VerifyEmail", model);
                }

                if (await _userManager.IsEmailConfirmedAsync(user))
                {
                    // E-posta zaten doğrulanmışsa
                    return RedirectToAction(nameof(Login));
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    nameof(ConfirmEmail),
                    "Auth",
                    new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "E-posta Adresinizi Doğrulayın",
                    $"Lütfen e-posta adresinizi doğrulamak için <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya tıklayın</a>.");

                model.EmailSent = true;
                _logger.LogInformation("Doğrulama e-postası gönderildi: {Email}", model.Email);
            }

            return View("VerifyEmail", model);
        }

        #endregion

        #region Şifre Sıfırlama

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Kullanıcı bulunamasa bile güvenlik nedeniyle başarılı gibi göster
                    model.EmailSent = true;
                    return View("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    nameof(ResetPassword),
                    "Auth",
                    new { code = code, email = model.Email },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Şifre Sıfırlama",
                    $"Lütfen şifrenizi sıfırlamak için <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya tıklayın</a>.");

                model.EmailSent = true;
                _logger.LogInformation("Şifre sıfırlama e-postası gönderildi: {Email}", model.Email);
                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null)
            {
                return BadRequest("Şifre sıfırlama için bir kod sağlanmalıdır.");
            }

            var model = new ResetPasswordViewModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                Email = email
            };
            
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Kullanıcı bulunamasa bile güvenlik nedeniyle başarılı gibi göster
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("Şifre sıfırlandı: {Email}", model.Email);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        #endregion

        #region Profil İşlemleri

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"'{_userManager.GetUserId(User)}' ID'li kullanıcı bulunamadı.");
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                BirthDate = user.BirthDate,
                Location = user.Location,
                Website = user.Website
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"'{_userManager.GetUserId(User)}' ID'li kullanıcı bulunamadı.");
            }

            // Profil resmi yükleme
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var fileName = $"{user.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}_{model.ProfilePicture.FileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles", fileName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }
                
                user.ProfilePictureUrl = $"/uploads/profiles/{fileName}";
            }

            // Kullanıcı bilgilerini güncelle
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Bio = model.Bio;
            user.BirthDate = model.BirthDate;
            user.Location = model.Location;
            user.Website = model.Website;
            user.UpdatedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Kullanıcı profili güncellendi: {Email}", user.Email);
                TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"'{_userManager.GetUserId(User)}' ID'li kullanıcı bulunamadı.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("Kullanıcı şifresini değiştirdi: {Email}", user.Email);
            
            TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
            return RedirectToAction(nameof(ChangePassword));
        }

        #endregion

        #region Two-Factor Authentication

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("İki faktörlü kimlik doğrulama kullanıcısı alınamadı.");
            }

            var model = new TwoFactorAuthenticationViewModel { RememberMe = rememberMe };
            ViewData["ReturnUrl"] = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(TwoFactorAuthenticationViewModel model, bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException("İki faktörlü kimlik doğrulama kullanıcısı alınamadı.");
            }

            var authenticatorCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("İki faktörlü kimlik doğrulama başarılı: {UserId}", user.Id);
                return RedirectToLocal(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("Kullanıcı hesabı kilitlendi: {UserId}", user.Id);
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogWarning("Geçersiz iki faktörlü kimlik doğrulama kodu: {UserId}", user.Id);
                ModelState.AddModelError(string.Empty, "Geçersiz doğrulama kodu.");
                return View(model);
            }
        }

        #endregion

        #region Yardımcı Metodlar

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion
    }
}