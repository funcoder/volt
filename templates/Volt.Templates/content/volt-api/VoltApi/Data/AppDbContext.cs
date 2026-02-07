using Microsoft.EntityFrameworkCore;
using Volt.Data;

namespace VoltApi.Data;

public class AppDbContext : VoltDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
//#if (useSqlite)
            optionsBuilder.UseSqlite("Data Source=VoltApi_development.db");
//#endif
//#if (usePostgres)
            optionsBuilder.UseNpgsql("Host=localhost;Database=VoltApi_development;Username=postgres");
//#endif
//#if (useSqlServer)
            optionsBuilder.UseSqlServer("Server=.;Database=VoltApi_development;Trusted_Connection=true;TrustServerCertificate=true");
//#endif
        }

        base.OnConfiguring(optionsBuilder);
    }
}
