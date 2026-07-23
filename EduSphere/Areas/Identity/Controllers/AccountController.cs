using EduSphere.Areas.Identity.ViewModels;
using EduSphere.Models;
using EduSphere.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
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
        private readonly IStringLocalizer<SharedResource> _localizer;
       
        public AccountController(
                     UserManager<ApplicationUser> userManager,
                     SignInManager<ApplicationUser> signInManager,
                     IEmailService emailService,
                     IImageService imageService,
                     ILogger<AccountController> logger,
                     IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _imageService = imageService;
            _logger = logger;
            _localizer = localizer;
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
              _localizer["Email confirmed successfully for user {Email}"],
              user.Email);
            if (!result.Succeeded)
            {
                return View("Error");
            }


            TempData["Success"] =
               _localizer["Email verified successfully. You can login now."].ToString();

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #endregion


        #region Authentication
        #region Login

        [HttpGet]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", _localizer["Invalid email or password."]);

                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", _localizer["This account has been disabled."]);

                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError("", _localizer["Please verify your email before logging in."]);

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
                    _localizer["Failed login attempt for {Email}"],
                    model.Email);
                ModelState.AddModelError("", _localizer["Invalid email or password."]);
                return View(model);
            }

            _logger.LogInformation(
   _localizer["LOGIN | Email: {Email}"],
   user.Email);
            return RedirectToAction(nameof(RedirectToDashboard));
        }

        #endregion
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> LogoutNow()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        #region Logout

        [HttpGet]
        [Authorize]
        public IActionResult Logout()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost()
        {
            var user = await _userManager.GetUserAsync(User);

            await _signInManager.SignOutAsync();

            _logger.LogInformation(
                _localizer["LOGOUT | Email: {Email} | User logged out."],
                user?.Email);

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Register

        [HttpGet]
        public IActionResult Register()
        {
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
                ModelState.AddModelError("", _localizer["Email already exists."]);
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
                   _localizer["Register failed for {Email}"],
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
                    _localizer["Failed to assign role {Role} to {Email}"],
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
                _localizer["New user registered. Email: {Email}, UserType: {UserType}"],
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
                _localizer["Registration completed successfully. Please check your email to verify your account."].ToString();

            return RedirectToAction(nameof(Login));
        }

        #endregion

        #endregion


        #region Helpers

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return Content("Access denied.");
        }

        #region Redirect

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RedirectToDashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction(nameof(Login));

            return user.UserType switch
            {
                UserType.SuperAdmin => RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "SuperAdmin" }),

                UserType.CenterManager => RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "Center" }),

                UserType.Teacher => RedirectToAction(
                    "TeacherDashboard",
                    "Home",
                    new { area = "Center" }),

                UserType.Student => RedirectToAction(
                    "StudentDashboard",
                    "Home",
                    new { area = "Center" }),

                UserType.Parent => RedirectToAction(
                    "ParentDashboard",
                    "Home",
                    new { area = "Center" }),

                _ => RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "" })
            };
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
        _localizer["PROFILE UPDATED | Email: {Email}"],
        user.Email);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }

                TempData["Success"] = _localizer["Profile updated successfully."].ToString();

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
                    _localizer["Password changed successfully."].ToString();

                _logger.LogInformation(
        _localizer["CHANGE PASSWORD | Email: {Email}"],
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
                    ModelState.AddModelError("", _localizer["User not found."]);

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
                    _localizer["Password has been reset successfully. Please login."].ToString();
                _logger.LogInformation(
        _localizer["RESET PASSWORD | Email: {Email}"],
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
                        _localizer["If the email exists, a password reset link has been sent."].ToString();

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
        _localizer["FORGOT PASSWORD | Email: {Email}"],
        user.Email);
                TempData["Success"] =
                    _localizer["Password reset link has been sent to your email."].ToString();

                return RedirectToAction(nameof(Login));
            }

            #endregion




        }

    } 
