using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.AdminWebApp.Areas.Identity.Data;

public class IdentityDataContext : IdentityDbContext<IdentityUser>, IDataProtectionKeyContext
{
    public IdentityDataContext(DbContextOptions<IdentityDataContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
}
