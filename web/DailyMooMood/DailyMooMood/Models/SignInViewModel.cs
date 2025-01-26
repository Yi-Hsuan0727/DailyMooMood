using System.ComponentModel.DataAnnotations;

namespace DailyMooMood.Models
{
    public class SignInViewModel
    {
        private const string ErrorMessageEmail = "Enter a valid email. Check for typos and make sure the format is example@xyz.com.";
        private const string ErrorMessagePassword = "Password must be at least 8 characters.";

        [Required(ErrorMessage = ErrorMessageEmail)]
        [RegularExpression("^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$", ErrorMessage = ErrorMessageEmail)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = ErrorMessagePassword)]
        [MinLength(8, ErrorMessage = ErrorMessagePassword)]
        public string Password { get; set; } = null!;
    }
}
