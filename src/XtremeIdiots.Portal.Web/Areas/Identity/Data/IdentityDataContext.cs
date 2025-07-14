using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace XtremeIdiots.Portal.Web.Areas.Identity.Data;

/// <summary>
/// Entity Framework DbContext for ASP.NET Core Identity with data protection key storage.
/// Provides authentication and authorization data storage along with data protection keys for distributed scenarios.
/// </summary>
public class IdentityDataContext : IdentityDbContext<IdentityUser>, IDataProtectionKeyContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityDataContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public IdentityDataContext(DbContextOptions<IdentityDataContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// Override this method to further configure the model that was discovered by convention from the entity types
    /// exposed in <see cref="DbSet{TEntity}"/> properties on your derived context.
    /// </summary>
    /// <param name="builder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Additional identity configuration can be added here as needed
    }

    /// <summary>
    /// Gets or sets the DbSet for data protection keys used in distributed scenarios.
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}
