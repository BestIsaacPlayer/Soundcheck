using Microsoft.EntityFrameworkCore;
using Soundcheck.Web.Data;

namespace Soundcheck.Tests;

public static class TestDatabase
{
    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
