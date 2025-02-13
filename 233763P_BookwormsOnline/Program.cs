using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _233763P_BookwormsOnline.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using _233763P_BookwormsOnline.Services;
using Azure;
using Microsoft.Extensions.Options;


var sessionTimeout = TimeSpan.FromMinutes(30); // Define once and reuse
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthConnectionString")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 3; 
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Keys")))
    .SetApplicationName("BookwormsOnline");

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.ConfigureApplicationCookie(Config =>
{
    Config.LoginPath = "/Login";
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); //  Global CSRF protection
});

builder.Services.AddHttpClient(); // Required for API calls
builder.Services.AddScoped<ReCaptchaService>(); // Registers reCAPTCHA verification


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithRedirects("/Errors/{0}");

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
    await next();
});

app.UseStaticFiles();

app.UseRouting();

app.UseSession();


app.UseAuthentication();


app.UseAuthorization();


/*
app.Use(async (context, next) =>
{
    var userEmail = context.Session.GetString("UserEmail");

    if (!string.IsNullOrEmpty(userEmail))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Retrieve the actual session record
        var userSession = await dbContext.UserSessions.FirstOrDefaultAsync(s => s.UserEmail == userEmail);

        if (userSession != null)
        {
            logger.LogInformation("User session for {Email} found. Removing session as per middleware logic.", userEmail);

            // Remove the session record
            dbContext.UserSessions.Remove(userSession);
            await dbContext.SaveChangesAsync();

            context.Session.Clear();
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            context.Response.Redirect("/Login?timeout=true");
            return;
        }
        else
        {
            logger.LogInformation("No UserSession record found for {Email}", userEmail);
        }
    }
    await next();
});
*/


app.MapRazorPages();

app.Run();
