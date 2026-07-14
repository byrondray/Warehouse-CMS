using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Warehouse_CMS.Data;
using Xunit;

namespace Warehouse_CMS.Tests
{
    public class SeedDatabaseTests
    {
        // Builds a service provider wired with EF InMemory + Identity and a fake environment,
        // matching what SeedDatabase.SeedAsync resolves at runtime.
        private static ServiceProvider BuildProvider(string environmentName)
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var dbName = Guid.NewGuid().ToString();
            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));

            services
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var env = new Mock<IWebHostEnvironment>();
            env.SetupGet(e => e.EnvironmentName).Returns(environmentName);
            services.AddSingleton(env.Object);

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task SeedAsync_Throws_WhenAdminPasswordUnset_InProduction()
        {
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);
            using var provider = BuildProvider("Production");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => SeedDatabase.SeedAsync(provider)
            );
        }

        [Fact]
        public async Task SeedAsync_CreatesAdmin_WhenPasswordProvided_InProduction()
        {
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "Str0ng!Passw0rd");
            Environment.SetEnvironmentVariable("ADMIN_EMAIL", "admin@test.com");
            try
            {
                using var provider = BuildProvider("Production");

                await SeedDatabase.SeedAsync(provider);

                using var scope = provider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<
                    UserManager<IdentityUser>
                >();
                var admin = await userManager.FindByEmailAsync("admin@test.com");

                Assert.NotNull(admin);
                Assert.True(await userManager.IsInRoleAsync(admin!, "Admin"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);
                Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);
            }
        }

        [Fact]
        public async Task SeedAsync_UsesDevFallbackPassword_InDevelopment()
        {
            Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);
            using var provider = BuildProvider("Development");

            // Should not throw: development seeds a fallback password.
            await SeedDatabase.SeedAsync(provider);

            using var scope = provider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var admin = await userManager.FindByEmailAsync("admin@warehouse.com");

            Assert.NotNull(admin);
        }
    }
}
