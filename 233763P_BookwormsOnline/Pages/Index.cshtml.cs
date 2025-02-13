using _233763P_BookwormsOnline.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace _233763P_BookwormsOnline.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IDataProtector _protector;
        private readonly ILogger<IndexModel> _logger;
        private readonly AuthDbContext _context;

        public ApplicationUser UserProfile { get; private set; }
        public string MaskedCreditCard { get; private set; }
        public string AlertMessage { get; private set; }
        public string AlertType { get; private set; }

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dataProtectionProvider,
            AuthDbContext context,
            ILogger<IndexModel> logger)
        {
            this.userManager = userManager;
            _protector = dataProtectionProvider.CreateProtector("BookwormsOnline.CreditCards");
            _logger = logger;
            _context = context;
        }

        private string MaskCreditCard(string creditCard)
        {
            if (string.IsNullOrEmpty(creditCard)) return string.Empty;

            // Keep first 4 and last 4 digits visible
            if (creditCard.Length >= 8)
            {
                string firstFour = creditCard.Substring(0, 4);
                string lastFour = creditCard.Substring(creditCard.Length - 4);
                string masked = new string('*', creditCard.Length - 8);
                return $"{firstFour} {masked} {lastFour}";
            }
            return creditCard;
        }

        public async Task<IActionResult> OnGet()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToPage("Login");
                }

                UserProfile = user;
                



                if (!string.IsNullOrEmpty(user.CreditCard))
                {
                    try
                    {
                        _logger.LogInformation("Attempting to decrypt credit card");
                        var decryptedCard = _protector.Unprotect(user.CreditCard);
                        MaskedCreditCard = MaskCreditCard(decryptedCard);
                        _logger.LogInformation("Credit card decrypted and masked successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error decrypting credit card for user {UserId}", user.Id);
                        MaskedCreditCard = "Error retrieving card information";
                        AlertMessage = "There was an error retrieving your credit card information.";
                        AlertType = "warning";
                    }
                }
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                AlertMessage = "There was an error loading your profile.";
                AlertType = "danger";
                return Page();
            }
        }
    }
}
