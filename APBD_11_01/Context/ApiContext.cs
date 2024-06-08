using APBD_11_01.Models;
using Microsoft.EntityFrameworkCore;

namespace APBD_11_01.Context;

public partial class ApiContext : DbContext
{
    public virtual DbSet<AppUser> Users { get; set; }
    // public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public ApiContext()
    {
    }

    public ApiContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=APBD_11;User=sa;Password=fY0urP@sswor_Policy;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(user =>
            {
                user.HasKey(u => u.IdUser);
                user.Property(u => u.Username).IsRequired().HasMaxLength(100);
                user.Property(u => u.Password).IsRequired().HasMaxLength(100);
                user.Property(u => u.RefreshToken).IsRequired().HasMaxLength(100);
                // user.HasOne(u => u.RefreshToken)
                //     .WithOne(t => t.User)
                //     .HasForeignKey<RefreshToken>(u => u.IdToken);
            }
        );

        // modelBuilder.Entity<RefreshToken>(token =>
        //     {
        //         token.HasKey(t => t.IdToken);
        //         token.Property(t => t.Value).IsRequired().HasMaxLength(100);
        //         // token.HasOne(t => t.User)
        //         // .WithOne(u => u.RefreshToken)
        //         // .HasForeignKey<AppUser>(t => t.IdUser)
        //         // .IsRequired(false);
        //     }
        // );
    }
}