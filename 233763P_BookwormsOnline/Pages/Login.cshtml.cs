using _233763P_BookwormsOnline.ViewModels;
using _233763P_BookwormsOnline.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using _233763P_BookwormsOnline.Services;

namespace _233763P_BookwormsOnline.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public Login LModel { get; set; }

        public string ReCaptchaToken { get; set; } // Captures reCAPTCHA token
        private readonly ReCaptchaService _reCaptchaService; // Ensure this matches the constructor
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly AuthDbContext _context;

        public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginModel> logger,
        AuthDbContext context,
        ReCaptchaService reCaptchaService) //  Inject ReCaptchaService
        {
            this.signInManager = signInManager;
            _logger = logger;
            _context = context;
            _reCaptchaService = reCaptchaService; //  Assign it to class variable
        }
        public void OnGet()
        {
        }
        private async Task<bool> CheckExistingSession(string email)
        {
            try
            {
                var existingSession = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.UserEmail == email);
                return existingSession != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existing session for {Email}", email);
                throw;
            }
        }
        private async Task StoreSessionInDatabase(string email)
        {
            try
            {
                _context.UserSessions.Add(new UserSession { UserEmail = email });
                await _context.SaveChangesAsync();
                _logger.LogInformation("Session created for {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing session for {Email}", email);
                throw;
            }
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning("Validation error for {Key}: {ErrorMessage}", state.Key, error.ErrorMessage);
                    }
                }
                _logger.LogWarning("Model state is invalid for user {Email}", LModel.Email);
                return Page();
            }

            /*bool isHuman = await _reCaptchaService.VerifyTokenAsync(ReCaptchaToken);
            if (!isHuman)
            {
                _logger.LogWarning("reCAPTCHA verification failed for user {Email}", LModel.Email);
                ModelState.AddModelError("", "reCAPTCHA verification failed. Are you a bot?");
                return Page();
            }
            */

            try
            {
                _logger.LogInformation("Login attempt for {Email}", LModel.Email);

                bool hasExistingSession = await CheckExistingSession(LModel.Email);
                if (hasExistingSession)
                {
                    _logger.LogWarning("Existing session found for {Email}", LModel.Email);
                    ModelState.AddModelError("", "Account is already logged in from another device/browser.");
                    return Page();
                }

                var identityResult = await signInManager.PasswordSignInAsync(
                    LModel.Email,
                    LModel.Password,
                    LModel.RememberMe,
                    lockoutOnFailure: true);

                if (identityResult.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} is locked out.", LModel.Email);
                    ModelState.AddModelError("", "Your account has been locked due to multiple failed login attempts. Please try again in 15 minutes.");
                    return Page();
                }

                _logger.LogInformation("PasswordSignInAsync result for {Email}: Succeeded={Succeeded}, LockedOut={LockedOut}, NotAllowed={NotAllowed}, RequiresTwoFactor={RequiresTwoFactor}",
                    LModel.Email,
                    identityResult.Succeeded,
                    identityResult.IsLockedOut,
                    identityResult.IsNotAllowed,
                    identityResult.RequiresTwoFactor);

                if (identityResult.Succeeded)
                {
                    await StoreSessionInDatabase(LModel.Email);
                    _logger.LogInformation("User {Email} logged in successfully", LModel.Email);
                    return RedirectToPage("Index");
                }

                if (identityResult.IsLockedOut)
                {
                    _logger.LogWarning("User {Email} is locked out.", LModel.Email);
                    ModelState.AddModelError("", "User account is locked out.");
                }
                else if (identityResult.IsNotAllowed)
                {
                    _logger.LogWarning("User {Email} is not allowed to login.", LModel.Email);
                    ModelState.AddModelError("", "User is not allowed to login.");
                }
                else if (identityResult.RequiresTwoFactor)
                {
                    _logger.LogWarning("User {Email} requires two-factor authentication.", LModel.Email);
                    // Optionally handle the two-factor flow here.
                }
                else
                {
                    _logger.LogWarning("Invalid login attempt for {Email}.", LModel.Email);
                    ModelState.AddModelError("", "Username or Password incorrect");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process for {Email}", LModel.Email);
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
            }

            return Page();
        }

    }
}