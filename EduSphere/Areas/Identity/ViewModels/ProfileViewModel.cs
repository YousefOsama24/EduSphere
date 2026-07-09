using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using EduSphere.Models;

namespace EduSphere.Areas.Identity.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public UserType UserType { get; set; }

        public string? CurrentProfileImage { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? ProfileImage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}