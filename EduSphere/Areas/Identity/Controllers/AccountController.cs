using EduSphere.Areas.Identity.ViewModels;
using EduSphere.Models;
using EduSphere.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
namespace EduSphere.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IImageService _imageService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
                     UserManager<ApplicationUser> userManager,
                     SignInManager<ApplicationUser> signInManager,
                     IEmailService emailService,
                     IImageService imageService,
                     ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _imageService = imageService;
            _logger = logger;
        }


        #region Email Confirmation

        #region Confirm Email

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            string userId,
            string token)
        {
            if (string.IsNullOrEmpty(userId)
                || string.IsNullOrEmpty(token))
            {
                return BadRequest();
            }

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result =
                await _userManager.ConfirmEmailAsync(
                    user,
                    token);
            _logger.LogInformation(
    "Email confirmed successfully for user {Email}",
    user.Email);
            if (!result.Succeeded)
            {
                return View("Error");
            }


            TempData["Success"] =
                "Email verified successfully. You can login now.";

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #endregion


        #region Authentication
        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction(nameof(RedirectToDashboard));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");

                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "This account has been disabled.");

                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", "Please verify your email before logging in.");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);



            if (!result.Succeeded)
            {


                _logger.LogWarning(
    "Failed login attempt for {Email}",
    model.Email);
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            _logger.LogInformation(
   "LOGIN | Email: {Email}",
   user.Email);
            return RedirectToAction(nameof(RedirectToDashboard));
        }

        #endregion

        #region Logout

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);

            await _signInManager.SignOutAsync();

            _logger.LogInformation(
                "LOGOUT | Email: {Email} | User logged out.",
                user?.Email);

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction(nameof(RedirectToDashboard));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View(model);
            }

            ApplicationUser applicationUser = new()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                UserType = model.UserType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            IdentityResult result = await _userManager.CreateAsync(
                applicationUser,
                model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning(
                    "Register failed for {Email}",
                    model.Email);

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                applicationUser,
                applicationUser.UserType.ToString());

            if (!roleResult.Succeeded)
            {
                _logger.LogError(
                    "Failed to assign role {Role} to {Email}",
                    applicationUser.UserType,
                    applicationUser.Email);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                await _userManager.DeleteAsync(applicationUser);

                return View(model);
            }

            _logger.LogInformation(
                "New user registered. Email: {Email}, UserType: {UserType}",
                applicationUser.Email,
                applicationUser.UserType);

            var token =
                await _userManager.GenerateEmailConfirmationTokenAsync(
                    applicationUser);

            var confirmationLink = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new
                {
                    area = "Identity",
                    userId = applicationUser.Id,
                    token = token
                },
                Request.Scheme);

            await _emailService.SendConfirmationEmailAsync(
                applicationUser.Email!,
                applicationUser.FullName,
                HtmlEncoder.Default.Encode(confirmationLink!));

            TempData["Success"] =
                "Registration completed successfully. Please check your email to verify your account.";

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #endregion


        #region Helpers

        #region Redirect

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RedirectToDashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            switch (user.UserType)
            {
                case UserType.SuperAdmin:

                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "SuperAdmin" });

                case UserType.CenterManager:

                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Center" });

                case UserType.Teacher:

                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Teacher" });

                case UserType.Student:

                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Student" });

                case UserType.Parent:

                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "Parent" });

                default:

                    return RedirectToAction(
                        "Index",
                        "Home",
                        new { area = "" });
            }
        }

        #endregion

        #endregion

        #region Profile

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            ProfileViewModel model = new()
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                UserType = user.UserType,
                CurrentProfileImage = user.ProfileImage,
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.ProfileImage != null)
            {
                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    _imageService.DeleteImage(
                        user.ProfileImage,
                        "profiles");
                }

                user.ProfileImage =
                    await _imageService.UploadImageAsync(
                        model.ProfileImage,
                        "profiles");
            }

            var result = await _userManager.UpdateAsync(user);
            _logger.LogInformation(
    "PROFILE UPDATED | Email: {Email}",
    user.Email);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            TempData["Success"] = "Profile updated successfully.";

            return RedirectToAction(nameof(Profile));
        }

        #endregion

        #region Change Password

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] =
                "Password changed successfully.";

            _logger.LogInformation(
    "CHANGE PASSWORD | Email: {Email}",
    user.Email);

            return RedirectToAction(nameof(Profile));
        }

        #endregion
















        #region Reset Password

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
                return BadRequest();

            ResetPasswordViewModel model = new()
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");

                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            TempData["Success"] =
                "Password has been reset successfully. Please login.";
            _logger.LogInformation(
    "RESET PASSWORD | Email: {Email}",
    user.Email);

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Forgot Password

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["Success"] =
                    "If the email exists, a password reset link has been sent.";

                return RedirectToAction(nameof(Login));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetLink = Url.Action(
                nameof(ResetPassword),
                "Account",
                new
                {
                    area = "Identity",
                    token,
                    email = user.Email
                },
                Request.Scheme);

            await _emailService.SendResetPasswordEmailAsync(
                user.Email!,
                user.FullName,
                resetLink!);

            _logger.LogInformation(
    "FORGOT PASSWORD | Email: {Email}",
    user.Email);
            TempData["Success"] =
                "Password reset link has been sent to your email.";

            return RedirectToAction(nameof(Login));
        }

        #endregion




    }

}