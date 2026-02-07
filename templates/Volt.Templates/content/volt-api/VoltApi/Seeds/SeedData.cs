using Volt.Data;
using Volt.Data.Seeding;

namespace VoltApi.Seeds;

public class SeedData : VoltSeeder
{
    public override async Task SeedAsync(VoltDbContext context, CancellationToken cancellationToken = default)
    {
        // Add seed data here
        await Task.CompletedTask;
    }
}
