using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Nexus.Api.Auth;

public class AuthDbContext(DbContextOptions options) 
    : IdentityDbContext<User>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
            .Property(u => u.RestaurantId)
            .IsRequired();

        builder.HasDefaultSchema("identity");
    }
}