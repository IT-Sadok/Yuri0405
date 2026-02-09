using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Tests;

public static class TestDbContextFactory
{
    public static InsuranceDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InsuranceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new InsuranceDbContext(options);
    }
}
