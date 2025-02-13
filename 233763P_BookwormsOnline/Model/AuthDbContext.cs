using _233763P_BookwormsOnline.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace _233763P_BookwormsOnline.Model
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;

       
        public DbSet<UserSession> UserSessions { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;

        }

       

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = _configuration.GetConnectionString("AuthConnectionString");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✅ Ensure each user can have only one session entry
            builder.Entity<UserSession>()
                .HasKey(s => s.UserEmail);
        }
    }
}
