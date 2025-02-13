using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using System.Threading.Tasks;
using _233763P_BookwormsOnline.Model;
using Ganss.Xss; //  For HTML Sanitization
using System.Text.Encodings.Web; //  For Encoding

namespace _233763P_BookwormsOnline.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _protector;
        private readonly ILogger<RegisterModel> _logger;
        private readonly AuthDbContext _context;

        [BindProperty]
        public Register RModel { get; set; }

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IWebHostEnvironment webHostEnvironment,
            IDataProtectionProvider dataProtectionProvider,
            AuthDbContext context,
            ILogger<RegisterModel> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _protector = dataProtectionProvider.CreateProtector("BookwormsOnline.CreditCards");
            _context = context;
            _logger = logger;
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                //  Initialize sanitizers
                var sanitizer = new HtmlSanitizer();
                var encoder = HtmlEncoder.Default;

                //  Sanitize user inputs
                RModel.FirstName = sanitizer.Sanitize(RModel.FirstName).Trim();
                RModel.LastName = sanitizer.Sanitize(RModel.LastName).Trim();
                RModel.BillingAddress = sanitizer.Sanitize(RModel.BillingAddress).Trim();
                RModel.ShippingAddress = sanitizer.Sanitize(RModel.ShippingAddress).Trim();

                //  Encode email to prevent script injection
                RModel.Email = encoder.Encode(RModel.Email);

                //  Handle Photo Upload
                string uniqueFileName = null;
                if (RModel.Photo != null)
                {
                    if (Path.GetExtension(RModel.Photo.FileName).ToLower() != ".jpg")
                    {
                        ModelState.AddModelError("", "Only .JPG files are allowed.");
                        return Page();
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + RModel.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await RModel.Photo.CopyToAsync(fileStream);
                    }
                }

                //  Encrypt credit card (if provided)
                string encryptedCreditCard = null;
                try
                {
                    if (!string.IsNullOrEmpty(RModel.CreditCard))
                    {
                        encryptedCreditCard = _protector.Protect(RModel.CreditCard.Trim());
                        _logger.LogInformation("Credit card encrypted successfully");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error encrypting credit card");
                    ModelState.AddModelError("", "Error processing payment information.");
                    return Page();
                }

                //  Create user object
                var user = new ApplicationUser()
                {
                    FirstName = RModel.FirstName,
                    LastName = RModel.LastName,
                    CreditCard = encryptedCreditCard,
                    MobileNo = RModel.MobileNo,
                    BillingAddress = RModel.BillingAddress,
                    ShippingAddress = RModel.ShippingAddress,
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    PhotoPath = uniqueFileName
                };

                //  Save user to database
                var result = await userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    // Create a new UserSession using the unique email (only field)
                    var userSession = new UserSession
                    {
                        UserEmail = user.Email
                    };

                    // Save the session record to the database
                    _context.UserSessions.Add(userSession);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("New session created for {UserEmail}", user.Email);

                    // Store the user's email in the session so it can be retrieved later (e.g., for logout)
                    HttpContext.Session.SetString("UserEmail", user.Email);

                    return RedirectToPage("Index");
                }

                //  Handle errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in registration");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
            }

            return Page();

        }
    }
}
