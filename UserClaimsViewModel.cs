using System.Security.Claims;

namespace MyApp.ViewModels
{
    public class UserClaimsViewModel
    {
        public string UserId { get; set; }
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }
}