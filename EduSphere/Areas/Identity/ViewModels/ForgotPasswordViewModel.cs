using System.ComponentModel.DataAnnotations;

namespace EduSphere.Areas.Identity.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}