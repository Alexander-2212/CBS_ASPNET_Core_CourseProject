using System.ComponentModel.DataAnnotations;

namespace CBS_ASPNET_Core_CourseProject.Models
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароль і пароль підтвердження не збігаються.")]
        public string ConfirmPassword { get; set; }

        public bool WantsEmailNotifications { get; set; }
    }
}
