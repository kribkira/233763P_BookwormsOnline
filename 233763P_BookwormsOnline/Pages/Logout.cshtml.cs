using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _233763P_BookwormsOnline.Model;
using Microsoft.EntityFrameworkCore;
namespace _233763P_BookwormsOnline.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly AuthDbContext _context;
        private readonly ILogger<LogoutModel> _logger;
        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            AuthDbContext context,
            ILogger<LogoutModel> logger)
        {
            this.signInManager = signInManager;
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            try
            {
                var userEmail = HttpContext.Session.GetString("UserEmail");
                _logger.LogInformation("Logout requested for session email: {UserEmail}", userEmail);

                // Delete session directly, even if userEmail might be null
                var deletedSessions = await _context.UserSessions
                    .Where(s => s.UserEmail == userEmail)
                    .ExecuteDeleteAsync();
                _logger.LogInformation("Deleted {Count} session(s) for {Email}", deletedSessions, userEmail);

                HttpContext.Session.Clear(); // Clears in-memory session
                await signInManager.SignOutAsync(); // Clears authentication cookies
                Response.Cookies.Delete(".AspNetCore.Identity.Application"); // Forcefully remove authentication cookie
                return RedirectToPage("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToPage("Login"); // Even if there's an error, redirect to login
            }
        }
        public IActionResult OnPostDontLogoutAsync()
        {
            return RedirectToPage("Index");
        }
    }
}