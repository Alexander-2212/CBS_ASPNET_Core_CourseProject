using Microsoft.AspNetCore.Identity;

namespace CBS_ASPNET_Core_CourseProject.Entities
{
    public class User : IdentityUser
    {
        public bool WantsEmailNotifications { get; set; }
    }
}
