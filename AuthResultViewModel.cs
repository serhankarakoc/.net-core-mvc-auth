namespace MyApp.ViewModels
{
    public class AuthResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string RedirectUrl { get; set; }
        public string ErrorCode { get; set; }
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}