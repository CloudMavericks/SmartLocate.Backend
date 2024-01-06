using Microsoft.AspNetCore.Identity;
using SmartLocate.Admin.Entities;
using SmartLocate.Infrastructure.Commons.Repositories;

namespace SmartLocate.Admin.Services;

public class DataSeeder(IMongoRepository<AdminUser> mongoRepository, ILogger<DataSeeder> logger)
{
    public void SeedFirstAdminUser()
    {
        Task.Run(async () =>
        {
            var existingAdminUser = await mongoRepository.FirstOrDefaultAsync(x => x.IsSuperAdmin);
            if (existingAdminUser != null)
            {
                logger.LogInformation("First admin user has already been seeded");
                return;
            }

            var adminUser = new AdminUser
            {
                Name = "Super Admin",
                Email = "super_admin@gmail.com",
                PhoneNumber = "0123456789",
                IsSuperAdmin = true
            };
            var hasher = new PasswordHasher<AdminUser>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");
            await mongoRepository.CreateAsync(adminUser);

            logger.LogInformation("First admin user has been seeded");
        }).GetAwaiter().GetResult();
    }
}