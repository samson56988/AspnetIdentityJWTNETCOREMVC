namespace AspnetIdentityDemo.Models
{
    public class ResetPassword
    {
        public string token { get; set; }

        public string Email { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }
    }
}
