using System.ComponentModel.DataAnnotations;

namespace CBS_ASPNET_Core_CourseProject.Models
{
    public class AccountSettingsViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool WantsEmailNotifications { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Новий пароль і пароль підтвердження не збігаються.")]
        public string ConfirmNewPassword { get; set; }
    }
}
