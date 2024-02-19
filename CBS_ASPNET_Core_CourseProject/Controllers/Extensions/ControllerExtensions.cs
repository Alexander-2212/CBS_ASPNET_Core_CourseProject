using Microsoft.AspNetCore.Mvc;

namespace CBS_ASPNET_Core_CourseProject.Controllers.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult WithSuccess(this IActionResult result, string title, string message)
        {
            if (result is Controller controller)
            {
                controller.TempData["SuccessTitle"] = title;
                controller.TempData["SuccessMessage"] = message;
            }

            return result;
        }

        public static IActionResult WithError(this IActionResult result, string title, string message)
        {
            if (result is Controller controller)
            {
                controller.TempData["ErrorTitle"] = title;
                controller.TempData["ErrorMessage"] = message;
            }

            return result;
        }
    }
}
